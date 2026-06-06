using System;
using System.Security;
using System.Threading.Tasks;
using DocuArchi.Api.Infrastructure.Features;
using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Models.Models.GestorDocumental.EliminarDocumento;
using MiApp.Services.Service.GestorDocumental.EliminarDocumento;
using MiApp.Services.Service.GestorDocumental.EliminarDocumento.Errors;
using MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction;
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
        public async Task<ActionResult<AppResponses<StorageDeleteResult?>>> EliminarDocumento(
            [FromRoute] long idAlmacen,
            [FromQuery] string? nombreGabinete,
            [FromQuery] string? sourceModule)
        {
            var startedAt = DateTime.UtcNow;
            var requestId = ResolveRequestId();

            try
            {
                if (idAlmacen <= 0)
                {
                    return ErrorResult(DeleteErrorCatalog.RuntimeInvalid, requestId, "idAlmacen", "Debe ser mayor a cero");
                }

                if (string.IsNullOrWhiteSpace(nombreGabinete))
                {
                    return ErrorResult(DeleteErrorCatalog.RuntimeInvalid, requestId, "nombreGabinete", "NombreGabinete requerido");
                }

                if (!DeleteSourceModuleCatalog.TryNormalize(sourceModule, out var normalizedSourceModule))
                {
                    return ErrorResult(DeleteErrorCatalog.RuntimeInvalid, requestId, "sourceModule", "SourceModule invalido");
                }

                var aliasValidation = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!aliasValidation.Success || string.IsNullOrWhiteSpace(aliasValidation.ClaimValue))
                {
                    return ErrorResult(DeleteErrorCatalog.RuntimeInvalid, requestId, "defaulalias", "DefaultDbAlias requerido");
                }

                var usuarioIdValidation = _claimValidationService.ValidateClaim<string>("usuarioid");
                if (!usuarioIdValidation.Success || string.IsNullOrWhiteSpace(usuarioIdValidation.ClaimValue))
                {
                    return ErrorResult(DeleteErrorCatalog.RuntimeInvalid, requestId, "usuarioid", "UsuarioId requerido");
                }

                if (!long.TryParse(usuarioIdValidation.ClaimValue, out var usuarioId) || usuarioId <= 0)
                {
                    throw new SecurityException("Claim invalido: usuarioid");
                }

                if (!await _featureToggleService.IsEnabledAsync(DeleteStorageEngineFeatureName))
                {
                    return ErrorResult(
                        DeleteErrorCatalog.FeatureDisabled,
                        requestId,
                        DeleteStorageEngineFeatureName,
                        "DELETE_STORAGE_ENGINE_V2 = false");
                }

                var request = new StorageDeleteRequest
                {
                    IdAlmacen = idAlmacen,
                    NombreGabinete = nombreGabinete.Trim(),
                    SourceModule = normalizedSourceModule,
                    DefaultDbAlias = aliasValidation.ClaimValue.Trim(),
                    WorkflowDbAlias = ResolveWorkflowDbAlias(),
                    UsuarioId = usuarioId,
                    UsuarioLogin = ResolveLogin(),
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

                return ErrorResult(DeleteErrorCatalog.RuntimeInvalid, requestId, "usuarioid", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Delete API unexpected failure requestId={RequestId} idAlmacen={IdAlmacen}",
                    requestId,
                    idAlmacen);

                return ErrorResult(DeleteErrorCatalog.Unexpected, requestId, "delete", ex.Message);
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
            var code = result.errors?.Length > 0 && result.errors[0] is AppError appError
                ? appError.Code
                : DeleteErrorCatalog.Unexpected;

            return DeleteHttpResultMapper.Map(this, DeleteErrorResponseFactory.ResolveHttpStatus(code), result);
        }

        private ActionResult<AppResponses<StorageDeleteResult?>> ErrorResult(
            string code,
            string requestId,
            string field,
            string? technicalDetail)
        {
            var response = DeleteErrorResponseFactory.Build<StorageDeleteResult>(
                code,
                requestId,
                field,
                technicalDetail);

            return DeleteHttpResultMapper.Map(this, DeleteErrorResponseFactory.ResolveHttpStatus(code), response);
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

        private string ResolveWorkflowDbAlias()
        {
            return User.FindFirst("defaulaliaswf")?.Value?.Trim() ?? string.Empty;
        }

        private string ResolveLogin()
        {
            return User.FindFirst("login")?.Value?.Trim().ToUpperInvariant() ?? string.Empty;
        }

    }
}
