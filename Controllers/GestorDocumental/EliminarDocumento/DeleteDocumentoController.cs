using System;
using System.Security;
using System.Threading.Tasks;
using DocuArchi.Api.Infrastructure.Features;
using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Models.Models.GestorDocumental.EliminarDocumento;
using MiApp.Services.Service.GestorDocumental.EliminarDocumento;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using MiApp.Services.Service.SessionHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DocuArchi.Api.Controllers.GestorDocumental.EliminarDocumento
{
    [Route("api/gestor-documental/eliminar-documento")]
    [ApiController]
    [Authorize]
    public sealed class DeleteDocumentoController : ControllerBase
    {
        private const string DeleteStorageEngineFeatureName = "DELETE_STORAGE_ENGINE_V2";

        private readonly IClaimValidationService _claimValidationService;
        private readonly IEliminarDocumentoUseCase _useCase;
        private readonly IFeatureToggleService _featureToggleService;
        private readonly IIpHelper _ipHelper;
        private readonly ILogger<DeleteDocumentoController> _logger;

        public DeleteDocumentoController(
            IClaimValidationService claimValidationService,
            IEliminarDocumentoUseCase useCase,
            IFeatureToggleService featureToggleService,
            IIpHelper ipHelper,
            ILogger<DeleteDocumentoController> logger)
        {
            _claimValidationService = claimValidationService;
            _useCase = useCase;
            _featureToggleService = featureToggleService;
            _ipHelper = ipHelper;
            _logger = logger;
        }

        [HttpDelete("{idAlmacen:long}")]
        public async Task<ActionResult<AppResponses<StorageDeleteResult?>>> EliminarDocumento([FromRoute] long idAlmacen)
        {
            var startedAt = DateTime.UtcNow;
            var requestId = ResolveRequestId();

            try
            {
                if (idAlmacen <= 0)
                {
                    return BadRequest(Validation("idAlmacen", "Debe ser mayor a cero"));
                }

                var aliasValidation = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!aliasValidation.Success || string.IsNullOrWhiteSpace(aliasValidation.ClaimValue))
                {
                    return BadRequest(aliasValidation.Response);
                }

                var usuarioIdValidation = _claimValidationService.ValidateClaim<string>("usuarioid");
                if (!usuarioIdValidation.Success || string.IsNullOrWhiteSpace(usuarioIdValidation.ClaimValue))
                {
                    return BadRequest(usuarioIdValidation.Response);
                }

                if (!long.TryParse(usuarioIdValidation.ClaimValue, out var usuarioId) || usuarioId <= 0)
                {
                    throw new SecurityException("Claim invalido: usuarioid");
                }

                if (!await _featureToggleService.IsEnabledAsync(DeleteStorageEngineFeatureName))
                {
                    return BadRequest(new AppResponses<StorageDeleteResult?>
                    {
                        success = false,
                        message = "Delete StorageEngineV2 deshabilitado",
                        data = null,
                        meta = new AppMeta { Status = "feature_disabled" },
                        errors =
                        [
                            new AppError
                            {
                                Type = "FeatureFlag",
                                Field = DeleteStorageEngineFeatureName,
                                Message = "DELETE_STORAGE_ENGINE_V2 = false"
                            }
                        ]
                    });
                }

                var request = new StorageDeleteRequest
                {
                    IdAlmacen = idAlmacen,
                    DefaultDbAlias = aliasValidation.ClaimValue.Trim(),
                    UsuarioId = usuarioId,
                    RequestId = requestId,
                    IpAddress = ResolveIpTrans()
                };

                _logger.LogInformation(
                    "Delete API request received requestId={RequestId} idAlmacen={IdAlmacen} usuarioId={UsuarioId}",
                    request.RequestId,
                    request.IdAlmacen,
                    request.UsuarioId);

                var result = await _useCase.ExecuteAsync(request);

                if (result.success)
                {
                    return Ok(result);
                }

                return MapErrorResult(result);
            }
            catch (SecurityException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Delete API security validation failed requestId={RequestId} idAlmacen={IdAlmacen}",
                    requestId,
                    idAlmacen);

                return BadRequest(new AppResponses<StorageDeleteResult?>
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
                _logger.LogError(
                    ex,
                    "Delete API unexpected failure requestId={RequestId} idAlmacen={IdAlmacen}",
                    requestId,
                    idAlmacen);

                return StatusCode(StatusCodes.Status500InternalServerError, new AppResponses<StorageDeleteResult?>
                {
                    success = false,
                    message = "Error interno procesando eliminación documental",
                    data = null,
                    meta = new AppMeta { Status = "error" },
                    errors =
                    [
                        new AppError
                        {
                            Type = "Exception",
                            Field = "delete",
                            Message = "Ocurrio un error no controlado en el endpoint de eliminar documento"
                        }
                    ]
                });
            }
            finally
            {
                var durationMs = (DateTime.UtcNow - startedAt).TotalMilliseconds;
                _logger.LogInformation(
                    "Delete API request completed requestId={RequestId} idAlmacen={IdAlmacen} durationMs={DurationMs}",
                    requestId,
                    idAlmacen,
                    durationMs);
            }
        }

        private ActionResult<AppResponses<StorageDeleteResult?>> MapErrorResult(AppResponses<StorageDeleteResult?> result)
        {
            var status = result.meta?.Status?.Trim().ToLowerInvariant();
            return status switch
            {
                "validation" => BadRequest(result),
                "forbidden" => StatusCode(StatusCodes.Status403Forbidden, result),
                "not_found" => NotFound(result),
                "business" => StatusCode(StatusCodes.Status409Conflict, result),
                _ => StatusCode(StatusCodes.Status500InternalServerError, result)
            };
        }

        private string ResolveRequestId()
        {
            if (Request.Headers.TryGetValue("X-Request-Id", out var requestIdHeader)
                && !string.IsNullOrWhiteSpace(requestIdHeader))
            {
                return requestIdHeader.ToString().Trim();
            }

            if (Request.Headers.TryGetValue("requestId", out var legacyRequestIdHeader)
                && !string.IsNullOrWhiteSpace(legacyRequestIdHeader))
            {
                return legacyRequestIdHeader.ToString().Trim();
            }

            return Guid.NewGuid().ToString("N");
        }

        private string ResolveIpTrans()
        {
            var ip = _ipHelper.ObtenerDireccionIP(HttpContext);
            return ip.StartsWith("#", StringComparison.Ordinal) ? string.Empty : ip;
        }

        private static AppResponses<StorageDeleteResult?> Validation(string field, string message)
        {
            return new AppResponses<StorageDeleteResult?>
            {
                success = false,
                message = message,
                data = null,
                meta = new AppMeta { Status = "validation" },
                errors =
                [
                    new AppError
                    {
                        Type = "Validation",
                        Field = field,
                        Message = message
                    }
                ]
            };
        }
    }
}
