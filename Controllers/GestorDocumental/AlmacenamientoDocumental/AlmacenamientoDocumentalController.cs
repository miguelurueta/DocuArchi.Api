using System;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using DocuArchi.Api.Infrastructure.Features;
using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.GestorDocumental.AlmacenamientoDocumental;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DocuArchi.Api.Controllers.GestorDocumental.AlmacenamientoDocumental
{
    [Route("api/gestor-documental/almacenamiento")]
    [ApiController]
    [Authorize]
    public sealed class AlmacenamientoDocumentalController : ControllerBase
    {
        private const string StorageEngineFeatureName = "StorageEngineV2";

        private readonly IClaimValidationService _claimValidationService;
        private readonly IAlmacenarDocumentoUseCase _useCase;
        private readonly IFeatureToggleService _featureToggleService;
        private readonly ILogger<AlmacenamientoDocumentalController> _logger;

        public AlmacenamientoDocumentalController(
            IClaimValidationService claimValidationService,
            IAlmacenarDocumentoUseCase useCase,
            IFeatureToggleService featureToggleService,
            ILogger<AlmacenamientoDocumentalController> logger)
        {
            _claimValidationService = claimValidationService;
            _useCase = useCase;
            _featureToggleService = featureToggleService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<AppResponses<AlmacenarDocumentoResponse?>>> AlmacenarDocumento(
            [FromBody] AlmacenarDocumentoRequest request)
        {
            try
            {
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

                if (!int.TryParse(usuarioIdValidation.ClaimValue, out var usuarioId) || usuarioId <= 0)
                {
                    throw new SecurityException("Claim invalido: usuarioid");
                }

                if (!await _featureToggleService.IsEnabledAsync(StorageEngineFeatureName))
                {
                    return BadRequest(new AppResponses<AlmacenarDocumentoResponse?>
                    {
                        success = false,
                        message = "StorageEngineV2 deshabilitado y no existe adaptador legacy configurado",
                        data = null,
                        meta = new AppMeta { Status = "feature_disabled" },
                        errors =
                        [
                            new AppError
                            {
                                Type = "FeatureFlag",
                                Field = StorageEngineFeatureName,
                                Message = "StorageEngineV2 = false"
                            }
                        ]
                    });
                }

                var usuario = ResolveUsuario();
                _logger.LogInformation(
                    "Storage API request received. usuarioId={UsuarioId}",
                    usuarioId);

                var result = await _useCase.ExecuteAsync(
                    request,
                    aliasValidation.ClaimValue,
                    usuario,
                    usuarioId);

                if (!result.success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (SecurityException ex)
            {
                _logger.LogWarning(ex, "Storage API security validation failed");
                return BadRequest(new AppResponses<AlmacenarDocumentoResponse?>
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
                _logger.LogError(ex, "Storage API unexpected failure");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new AppResponses<AlmacenarDocumentoResponse?>
                    {
                        success = false,
                        message = "Error interno procesando almacenamiento documental",
                        data = null,
                        meta = new AppMeta { Status = "error" },
                        errors =
                        [
                            new AppError
                            {
                                Type = "Exception",
                                Field = "storage",
                                Message = "Ocurrio un error no controlado en el endpoint de almacenamiento"
                            }
                        ]
                    });
            }
        }

        private string ResolveUsuario()
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
    }
}
