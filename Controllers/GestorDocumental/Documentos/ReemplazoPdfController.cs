using System;
using System.Linq;
using System.Security;
using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload;
using MiApp.DTOs.DTOs.GestorDocumental.Documentos.ReemplazoPdf;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Models.Models.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload;
using MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload;
using MiApp.Services.Service.GestorDocumental.Documentos.ReemplazoPdf;
using MiApp.Services.Service.SessionHelper;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.GestorDocumental.Documentos
{
    [Route("api/gestor-documental/documentos/reemplazopdf")]
    [ApiController]
    [Authorize]
    public sealed class ReemplazoPdfController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IReemplazoPdfService _reemplazoPdfService;
        private readonly IStorageLargeUploadService _uploadService;
        private readonly IIpHelper _ipHelper;
        private readonly ILogger<ReemplazoPdfController> _logger;

        public ReemplazoPdfController(
            IClaimValidationService claimValidationService,
            IReemplazoPdfService reemplazoPdfService,
            IStorageLargeUploadService uploadService,
            IIpHelper ipHelper,
            ILogger<ReemplazoPdfController> logger)
        {
            _claimValidationService = claimValidationService;
            _reemplazoPdfService = reemplazoPdfService;
            _uploadService = uploadService;
            _ipHelper = ipHelper;
            _logger = logger;
        }

        [HttpPost("upload-temporal/init")]
        public async Task<ActionResult<AppResponses<StorageUploadInitResponseDto?>>> InitUploadTemporal(
            [FromBody] StorageUploadInitRequestDto request)
        {
            try
            {
                var usuarioId = ResolveUsuarioId();
                var result = await _uploadService.InitAsync(request, usuarioId);
                return Ok(new AppResponses<StorageUploadInitResponseDto?>
                {
                    success = true,
                    message = "OK",
                    data = new StorageUploadInitResponseDto
                    {
                        RutaTemporalId = result.RutaTemporalId,
                        ArchivoTemporalId = result.ArchivoTemporalId,
                        ChunkSizeBytes = result.ChunkSizeBytes,
                        Estado = result.Estado
                    },
                    errors = Array.Empty<object>()
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "InitUploadTemporal reemplazo failed");
                return BadRequest(UploadError<StorageUploadInitResponseDto>(ex.Message));
            }
        }

        [HttpPut("upload-temporal/{rutaTemporalId}/{archivoTemporalId}/chunk/{chunkIndex:int}")]
        [Consumes("application/octet-stream")]
        public async Task<ActionResult<AppResponses<object?>>> UploadTemporalChunk(
            [FromRoute] string rutaTemporalId,
            [FromRoute] string archivoTemporalId,
            [FromRoute] int chunkIndex)
        {
            try
            {
                var usuarioId = ResolveUsuarioId();

                if (!Request.ContentLength.HasValue || Request.ContentLength.Value <= 0)
                {
                    return BadRequest(UploadError<object>("UPLOAD_INVALID_CHUNK", "Content-Length requerido"));
                }

                if (!Request.Headers.TryGetValue("X-Total-Chunks", out var totalChunksValues)
                    || !int.TryParse(totalChunksValues.ToString(), out var totalChunks)
                    || totalChunks <= 0)
                {
                    return BadRequest(UploadError<object>("UPLOAD_INVALID_CHUNK", "Header X-Total-Chunks requerido"));
                }

                await _uploadService.UploadChunkAsync(
                    rutaTemporalId,
                    archivoTemporalId,
                    chunkIndex,
                    totalChunks,
                    Request.ContentLength.Value,
                    Request.Body,
                    usuarioId);

                return Ok(new AppResponses<object?>
                {
                    success = true,
                    message = "OK",
                    data = new { chunkIndex },
                    errors = Array.Empty<object>()
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "UploadTemporalChunk reemplazo failed rutaTemporalId={RutaTemporalId} archivoTemporalId={ArchivoTemporalId} chunk={ChunkIndex}",
                    rutaTemporalId,
                    archivoTemporalId,
                    chunkIndex);
                return BadRequest(UploadError<object>(ex.Message));
            }
        }

        [HttpGet("upload-temporal/{rutaTemporalId}/{archivoTemporalId}/status")]
        public async Task<ActionResult<AppResponses<StorageUploadStatusResponseDto?>>> UploadTemporalStatus(
            [FromRoute] string rutaTemporalId,
            [FromRoute] string archivoTemporalId)
        {
            try
            {
                var usuarioId = ResolveUsuarioId();
                var status = await _uploadService.GetStatusAsync(rutaTemporalId, archivoTemporalId, usuarioId);
                return Ok(new AppResponses<StorageUploadStatusResponseDto?>
                {
                    success = true,
                    message = "OK",
                    data = new StorageUploadStatusResponseDto
                    {
                        Estado = status.Estado,
                        ChunksRecibidos = status.ChunksRecibidos,
                        ChunksPendientes = status.ChunksPendientes,
                        TamanoRecibidoBytes = status.TamanoRecibidoBytes
                    },
                    errors = Array.Empty<object>()
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "UploadTemporalStatus reemplazo failed rutaTemporalId={RutaTemporalId} archivoTemporalId={ArchivoTemporalId}",
                    rutaTemporalId,
                    archivoTemporalId);
                return BadRequest(UploadError<StorageUploadStatusResponseDto>(ex.Message));
            }
        }

        [HttpPost("upload-temporal/{rutaTemporalId}/{archivoTemporalId}/complete")]
        public async Task<ActionResult<AppResponses<object?>>> CompleteUploadTemporal(
            [FromRoute] string rutaTemporalId,
            [FromRoute] string archivoTemporalId)
        {
            try
            {
                var usuarioId = ResolveUsuarioId();
                await _uploadService.CompleteAsync(rutaTemporalId, archivoTemporalId, usuarioId);

                return Ok(new AppResponses<object?>
                {
                    success = true,
                    message = "OK",
                    data = new { Estado = StorageTemporaryUploadState.Completed },
                    errors = Array.Empty<object>()
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "CompleteUploadTemporal reemplazo failed rutaTemporalId={RutaTemporalId} archivoTemporalId={ArchivoTemporalId}",
                    rutaTemporalId,
                    archivoTemporalId);
                return BadRequest(UploadError<object>(ex.Message));
            }
        }

        [HttpDelete("upload-temporal/{rutaTemporalId}/{archivoTemporalId}")]
        public async Task<ActionResult<AppResponses<object?>>> CancelUploadTemporal(
            [FromRoute] string rutaTemporalId,
            [FromRoute] string archivoTemporalId)
        {
            try
            {
                var usuarioId = ResolveUsuarioId();
                await _uploadService.CancelAsync(rutaTemporalId, archivoTemporalId, usuarioId);

                return Ok(new AppResponses<object?>
                {
                    success = true,
                    message = "OK",
                    data = new { Estado = StorageTemporaryUploadState.Cancelled },
                    errors = Array.Empty<object>()
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "CancelUploadTemporal reemplazo failed rutaTemporalId={RutaTemporalId} archivoTemporalId={ArchivoTemporalId}",
                    rutaTemporalId,
                    archivoTemporalId);
                return BadRequest(UploadError<object>(ex.Message));
            }
        }

        [HttpPost]
        public async Task<ActionResult<AppResponses<ReemplazarDocumentoPdfResponse?>>> Post(
            [FromBody] ReemplazarDocumentoPdfRequest request)
        {
            try
            {
                var aliasValidation = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!aliasValidation.Success || string.IsNullOrWhiteSpace(aliasValidation.ClaimValue))
                {
                    return BadRequest(aliasValidation.Response);
                }

                var usuarioId = ResolveUsuarioId();
                var usuario = ResolveUsuario();
                var ipTrans = ResolveIpTrans();

                var result = await _reemplazoPdfService.ExecuteAsync(
                    request,
                    aliasValidation.ClaimValue,
                    usuario,
                    usuarioId,
                    ipTrans);

                if (!result.success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (SecurityException ex)
            {
                _logger.LogWarning(ex, "Replace PDF security validation failed");
                return BadRequest(new AppResponses<ReemplazarDocumentoPdfResponse?>
                {
                    success = false,
                    message = "Error de validacion de seguridad",
                    data = null,
                    meta = new AppMeta { Status = "validation" },
                    errors =
                    [
                        new AppError
                        {
                            Type = "Security",
                            Field = "usuarioid",
                            Message = ex.Message
                        }
                    ]
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Replace PDF unexpected failure");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new AppResponses<ReemplazarDocumentoPdfResponse?>
                    {
                        success = false,
                        message = "Error interno reemplazando documento",
                        data = null,
                        meta = new AppMeta { Status = "error" },
                        errors =
                        [
                            new AppError
                            {
                                Type = "Exception",
                                Field = "reemplazopdf",
                                Message = "Ocurrio un error no controlado en reemplazo de documento PDF"
                            }
                        ]
                    });
            }
        }

        private int ResolveUsuarioId()
        {
            try
            {
                var usuarioIdValidation = _claimValidationService.ValidateClaim<string>("usuarioid");
                if (!usuarioIdValidation.Success || string.IsNullOrWhiteSpace(usuarioIdValidation.ClaimValue))
                {
                    throw new SecurityException("Claim invalido: usuarioid");
                }

                if (!int.TryParse(usuarioIdValidation.ClaimValue, out var usuarioId) || usuarioId <= 0)
                {
                    throw new SecurityException("Claim invalido: usuarioid");
                }

                return usuarioId;
            }
            catch (SecurityException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new SecurityException($"Error resolviendo usuarioid: {ex.Message}");
            }
        }

        private static AppResponses<T?> UploadError<T>(string message, string? details = null) where T : class
        {
            return new AppResponses<T?>
            {
                success = false,
                message = "Error en upload temporal",
                data = null,
                meta = new AppMeta { Status = "validation" },
                errors =
                [
                    new AppError
                    {
                        Type = "Validation",
                        Field = message,
                        Message = details ?? message
                    }
                ]
            };
        }

        private string ResolveUsuario()
        {
            try
            {
                var fromIdentity = User?.Identity?.Name;
                if (!string.IsNullOrWhiteSpace(fromIdentity))
                {
                    return fromIdentity.Trim();
                }

                var fromClaim = User?.Claims?
                    .FirstOrDefault(c => string.Equals(c.Type, "usuario", StringComparison.OrdinalIgnoreCase))?
                    .Value;

                if (!string.IsNullOrWhiteSpace(fromClaim))
                {
                    return fromClaim.Trim();
                }

                var fromSubject = User?.Claims?
                    .FirstOrDefault(c => string.Equals(c.Type, "sub", StringComparison.OrdinalIgnoreCase))?
                    .Value;

                if (!string.IsNullOrWhiteSpace(fromSubject))
                {
                    return fromSubject.Trim();
                }

                return "usuario_autenticado";
            }
            catch
            {
                return "usuario_autenticado";
            }
        }

        private string ResolveIpTrans()
        {
            try
            {
                var ip = _ipHelper.ObtenerDireccionIP(HttpContext);
                return ip.StartsWith("#", StringComparison.Ordinal) ? string.Empty : ip;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
