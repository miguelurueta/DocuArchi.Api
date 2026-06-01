using System;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using DocuArchi.Api.Infrastructure.Features;
using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.GestorDocumental.AlmacenamientoDocumental;
using MiApp.DTOs.DTOs.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental;
using MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Errors;
using MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Observability;
using MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload;
using MiApp.Services.Service.SessionHelper;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MiApp.Models.Models.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload;

namespace DocuArchi.Api.Controllers.GestorDocumental.AlmacenamientoDocumental
{
    [Route("api/gestor-documental/almacenamiento")]
    [ApiController]
    [Authorize]
    public sealed class AlmacenamientoDocumentalController : ControllerBase
    {
        private const string StorageEngineFeatureName = "StorageEngineV2";

        private readonly IClaimValidationService _claimValidationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAlmacenarDocumentoUseCase _useCase;
        private readonly IStorageLargeUploadService _uploadService;
        private readonly IFeatureToggleService _featureToggleService;
        private readonly IIpHelper _ipHelper;
        private readonly ILogger<AlmacenamientoDocumentalController> _logger;

        public AlmacenamientoDocumentalController(
            IClaimValidationService claimValidationService,
            ICurrentUserService currentUserService,
            IAlmacenarDocumentoUseCase useCase,
            IStorageLargeUploadService uploadService,
            IFeatureToggleService featureToggleService,
            IIpHelper ipHelper,
            ILogger<AlmacenamientoDocumentalController> logger)
        {
            _claimValidationService = claimValidationService;
            _currentUserService = currentUserService;
            _useCase = useCase;
            _uploadService = uploadService;
            _featureToggleService = featureToggleService;
            _ipHelper = ipHelper;
            _logger = logger;
        }

        [HttpPost("upload-temporal/init")]
        public async Task<ActionResult<AppResponses<StorageUploadInitResponseDto?>>> InitUploadTemporal(
            [FromBody] StorageUploadInitRequestDto request)
        {
            try
            {
                var usuarioId = _currentUserService.ResolveUsuarioIdOrThrow();
                //var usuarioId = 141;
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
                _logger.LogWarning(ex, "InitUploadTemporal failed");
                return BadRequest(UploadError<StorageUploadInitResponseDto>(
                    StorageErrorCatalog.TemporaryUploadFailed,
                    "uploadTemporal.init",
                    ex.Message));
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
                var usuarioId = _currentUserService.ResolveUsuarioIdOrThrow();
                //var usuarioId = 141;

                if (!Request.ContentLength.HasValue || Request.ContentLength.Value <= 0)
                {
                    return BadRequest(UploadError<object>(
                        StorageErrorCatalog.InvalidChunk,
                        "Content-Length",
                        "Content-Length requerido"));
                }

                if (!Request.Headers.TryGetValue("X-Total-Chunks", out var totalChunksValues)
                    || !int.TryParse(totalChunksValues.ToString(), out var totalChunks)
                    || totalChunks <= 0)
                {
                    return BadRequest(UploadError<object>(
                        StorageErrorCatalog.InvalidChunk,
                        "X-Total-Chunks",
                        "Header X-Total-Chunks requerido"));
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
                _logger.LogWarning(ex, "UploadTemporalChunk failed rutaTemporalId={RutaTemporalId} archivoTemporalId={ArchivoTemporalId} chunk={ChunkIndex}", rutaTemporalId, archivoTemporalId, chunkIndex);
                return BadRequest(UploadError<object>(
                    StorageErrorCatalog.TemporaryUploadFailed,
                    "uploadTemporal.chunk",
                    ex.Message));
            }
        }

        [HttpGet("upload-temporal/{rutaTemporalId}/{archivoTemporalId}/status")]
        public async Task<ActionResult<AppResponses<StorageUploadStatusResponseDto?>>> UploadTemporalStatus(
            [FromRoute] string rutaTemporalId,
            [FromRoute] string archivoTemporalId)
        {
            try
            {
                var usuarioId = _currentUserService.ResolveUsuarioIdOrThrow();
                //var usuarioId = 141;
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
                _logger.LogWarning(ex, "UploadTemporalStatus failed rutaTemporalId={RutaTemporalId} archivoTemporalId={ArchivoTemporalId}", rutaTemporalId, archivoTemporalId);
                return BadRequest(UploadError<StorageUploadStatusResponseDto>(
                    StorageErrorCatalog.TemporaryUploadFailed,
                    "uploadTemporal.status",
                    ex.Message));
            }
        }

        [HttpPost("upload-temporal/{rutaTemporalId}/{archivoTemporalId}/complete")]
        public async Task<ActionResult<AppResponses<object?>>> CompleteUploadTemporal(
            [FromRoute] string rutaTemporalId,
            [FromRoute] string archivoTemporalId)
        {
            try
            {
                var usuarioId = _currentUserService.ResolveUsuarioIdOrThrow();
                //var usuarioId = 141;
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
                _logger.LogWarning(ex, "CompleteUploadTemporal failed rutaTemporalId={RutaTemporalId} archivoTemporalId={ArchivoTemporalId}", rutaTemporalId, archivoTemporalId);
                return BadRequest(UploadError<object>(
                    StorageErrorCatalog.TemporaryUploadFailed,
                    "uploadTemporal.complete",
                    ex.Message));
            }
        }

        [HttpDelete("upload-temporal/{rutaTemporalId}/{archivoTemporalId}")]
        public async Task<ActionResult<AppResponses<object?>>> CancelUploadTemporal(
            [FromRoute] string rutaTemporalId,
            [FromRoute] string archivoTemporalId)
        {
            try
            {
                var usuarioId = _currentUserService.ResolveUsuarioIdOrThrow();
                //var usuarioId = 141;
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
                _logger.LogWarning(ex, "CancelUploadTemporal failed rutaTemporalId={RutaTemporalId} archivoTemporalId={ArchivoTemporalId}", rutaTemporalId, archivoTemporalId);
                return BadRequest(UploadError<object>(
                    StorageErrorCatalog.TemporaryUploadFailed,
                    "uploadTemporal.cancel",
                    ex.Message));
            }
        }

        [HttpPost]
        public async Task<ActionResult<AppResponses<AlmacenarDocumentoResponse?>>> AlmacenarDocumento(
            [FromBody] AlmacenarDocumentoRequest request)
        {
            try
            {
                var perfContext = BuildPerfContext(request, null);
                return await StorageStageTimer.MeasureAsync(
                    _logger,
                    "api.total",
                    perfContext,
                    async () =>
                    {
                        var aliasValidation = StorageStageTimer.Measure(
                            _logger,
                            "api.claims",
                            perfContext,
                            () => _claimValidationService.ValidateClaim<string>("defaulalias"));
                        if (!aliasValidation.Success || string.IsNullOrWhiteSpace(aliasValidation.ClaimValue))
                        {
                            return BadRequest(StorageErrorResponseFactory.Build<AlmacenarDocumentoResponse>(
                                StorageErrorCatalog.AliasRequired,
                                request?.RequestId,
                                "defaulalias",
                                aliasValidation.Response?.message));
                        }

                        var usuarioId = StorageStageTimer.Measure(
                            _logger,
                            "api.claims",
                            perfContext,
                            () => _currentUserService.ResolveUsuarioIdOrThrow());

                        var authenticatedPerfContext = BuildPerfContext(request, usuarioId);

                        var featureEnabled = await StorageStageTimer.MeasureAsync(
                            _logger,
                            "api.featureFlag",
                            authenticatedPerfContext,
                            () => _featureToggleService.IsEnabledAsync(StorageEngineFeatureName));
                        if (!featureEnabled)
                        {
                            return BadRequest(StorageErrorResponseFactory.Build<AlmacenarDocumentoResponse>(
                                StorageErrorCatalog.FeatureDisabled,
                                request?.RequestId,
                                StorageEngineFeatureName));
                        }

                        var usuario = StorageStageTimer.Measure(
                            _logger,
                            "api.claims",
                            authenticatedPerfContext,
                            () => _currentUserService.ResolveLoginOrThrow());
                        var ipTrans = ResolveIpTrans();
                        _logger.LogInformation(
                            "Storage API request received. usuarioId={UsuarioId}",
                            usuarioId);

                        var result = await StorageStageTimer.MeasureAsync(
                            _logger,
                            "api.useCase",
                            authenticatedPerfContext,
                            () => _useCase.ExecuteAsync(
                                request,
                                aliasValidation.ClaimValue,
                                usuario,
                                usuarioId,
                                ipTrans));

                        if (!result.success)
                        {
                            return ToErrorActionResult(result);
                        }

                        return Ok(result);
                    });
            }
            catch (SecurityException ex)
            {
                _logger.LogWarning(ex, "Storage API security validation failed");
                return BadRequest(StorageErrorResponseFactory.Build<AlmacenarDocumentoResponse>(
                    StorageErrorCatalog.UserUnresolved,
                    request?.RequestId,
                    "usuarioid",
                    ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Storage API unexpected failure");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    StorageErrorResponseFactory.Build<AlmacenarDocumentoResponse>(
                        StorageErrorCatalog.UnexpectedError,
                        request?.RequestId,
                        "storage",
                        ex.Message));
            }
        }

        private ActionResult<AppResponses<AlmacenarDocumentoResponse?>> ToErrorActionResult(
            AppResponses<AlmacenarDocumentoResponse?> result)
        {
            var code = ResolveFirstErrorCode(result);
            var status = code == null
                ? StatusCodes.Status400BadRequest
                : StorageErrorResponseFactory.ResolveHttpStatus(code);

            if (status >= StatusCodes.Status500InternalServerError)
            {
                return StatusCode(status, result);
            }

            return BadRequest(result);
        }

        private static string? ResolveFirstErrorCode(AppResponses<AlmacenarDocumentoResponse?> result)
        {
            return result.errors?.OfType<AppError>().FirstOrDefault()?.Code;
        }

        private static AppResponses<T?> UploadError<T>(
            string code,
            string? field = null,
            string? details = null) where T : class
        {
            return StorageErrorResponseFactory.Build<T>(code, null, field, details);
        }

        private string ResolveIpTrans()
        {
            var ip = _ipHelper.ObtenerDireccionIP(HttpContext);
            return ip.StartsWith("#", StringComparison.Ordinal) ? string.Empty : ip;
        }

        private static StoragePerfContext BuildPerfContext(
            AlmacenarDocumentoRequest? request,
            int? usuarioId)
        {
            return new StoragePerfContext
            {
                RequestId = request?.RequestId,
                NombreGabinete = request?.NombreGabinete,
                UsuarioId = usuarioId,
                IdExpediente = request?.Expediente?.IdExpediente
            };
        }
    }
}
