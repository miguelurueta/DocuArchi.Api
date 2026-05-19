using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.GestorDocumental.PermisosVisorPdf;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.PermisosVisorPdf;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.GestorDocumental.PermisosVisorPdf
{
    [Route("api/gestor-documental/permisos-visorpdf")]
    [ApiController]
    [Authorize]
    public sealed class PermisosVisorPdfController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IPermisosVisorPdfService _service;

        public PermisosVisorPdfController(
            IClaimValidationService claimValidationService,
            ICurrentUserService currentUserService,
            IPermisosVisorPdfService service)
        {
            _claimValidationService = claimValidationService;
            _currentUserService = currentUserService;
            _service = service;
        }

        [HttpGet("implementaciones/{codigoImpl}/mis-permisos")]
        public async Task<ActionResult<AppResponses<VisorPdfPermissionsResponseDto>>> GetMyPermissions([FromRoute] string codigoImpl)
        {
            try
            {
                var aliasValidation = _claimValidationService.ValidateClaim<VisorPdfPermissionsResponseDto>("defaulalias");
                if (!aliasValidation.Success || string.IsNullOrWhiteSpace(aliasValidation.ClaimValue))
                {
                    return BadRequest(aliasValidation.Response);
                }

                var userValidation = _claimValidationService.ValidateClaim<VisorPdfPermissionsResponseDto>("usuarioid");
                if (!userValidation.Success || string.IsNullOrWhiteSpace(userValidation.ClaimValue))
                {
                    return BadRequest(userValidation.Response);
                }

                if (!int.TryParse(userValidation.ClaimValue, out var usuarioId) || usuarioId <= 0)
                {
                    return BadRequest(ValidationPermissions("usuarioid", "Claim invalido: usuarioid"));
                }

                var result = await _service.GetMyPermissionsAsync(codigoImpl, usuarioId, aliasValidation.ClaimValue);
                if (!result.success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorPermissions("GetMyPermissions", ex.Message));
            }
        }

        [HttpGet("implementaciones/{codigoImpl}/usuarios/{idUsuario}/permisos")]
        public async Task<ActionResult<AppResponses<VisorPdfPermissionsResponseDto>>> GetUserPermissions([FromRoute] string codigoImpl, [FromRoute] int idUsuario)
        {
            try
            {
                var aliasValidation = _claimValidationService.ValidateClaim<VisorPdfPermissionsResponseDto>("defaulalias");
                if (!aliasValidation.Success || string.IsNullOrWhiteSpace(aliasValidation.ClaimValue))
                {
                    return BadRequest(aliasValidation.Response);
                }

                if (!IsAdmin())
                {
                    return Unauthorized(ValidationPermissions("authorization", "No cuenta con permisos administrativos"));
                }

                var result = await _service.GetUserPermissionsAsync(codigoImpl, idUsuario, aliasValidation.ClaimValue);
                if (!result.success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorPermissions("GetUserPermissions", ex.Message));
            }
        }

        [HttpPut("implementaciones/{codigoImpl}/usuarios/{idUsuario}/overrides")]
        public async Task<ActionResult<AppResponses<SimpleOperationResultDto>>> UpsertOverrides([FromRoute] string codigoImpl, [FromRoute] int idUsuario, [FromBody] UpsertUserOverridesRequestDto request)
        {
            try
            {
                var aliasValidation = _claimValidationService.ValidateClaim<SimpleOperationResultDto>("defaulalias");
                if (!aliasValidation.Success || string.IsNullOrWhiteSpace(aliasValidation.ClaimValue))
                {
                    return BadRequest(aliasValidation.Response);
                }

                if (!IsAdmin())
                {
                    return Unauthorized(ValidationOperation("authorization", "No cuenta con permisos administrativos"));
                }

                var result = await _service.UpsertUserOverridesAsync(codigoImpl, idUsuario, request, aliasValidation.ClaimValue);
                if (!result.success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorOperation("UpsertOverrides", ex.Message));
            }
        }

        [HttpDelete("implementaciones/{codigoImpl}/usuarios/{idUsuario}/overrides/{codigoPermiso}")]
        public async Task<ActionResult<AppResponses<SimpleOperationResultDto>>> DeleteOverride([FromRoute] string codigoImpl, [FromRoute] int idUsuario, [FromRoute] string codigoPermiso)
        {
            try
            {
                var aliasValidation = _claimValidationService.ValidateClaim<SimpleOperationResultDto>("defaulalias");
                if (!aliasValidation.Success || string.IsNullOrWhiteSpace(aliasValidation.ClaimValue))
                {
                    return BadRequest(aliasValidation.Response);
                }

                if (!IsAdmin())
                {
                    return Unauthorized(ValidationOperation("authorization", "No cuenta con permisos administrativos"));
                }

                var result = await _service.DeleteUserOverrideAsync(codigoImpl, idUsuario, codigoPermiso, aliasValidation.ClaimValue);
                if (!result.success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ErrorOperation("DeleteOverride", ex.Message));
            }
        }

        private bool IsAdmin()
        {
            try
            {
                if (_currentUserService.HasPermission("pdf.permissions.admin"))
                {
                    return true;
                }

                var adminClaims = new[] { "esadmin", "isadmin", "admin", "role", "rol" };
                foreach (var claim in adminClaims)
                {
                    var value = _currentUserService.GetClaimValue(claim);
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        continue;
                    }

                    var normalized = value.Trim().ToLowerInvariant();
                    if (normalized is "1" or "true" or "admin" or "superadmin")
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static AppResponses<VisorPdfPermissionsResponseDto> ValidationPermissions(string field, string message)
        {
            return new AppResponses<VisorPdfPermissionsResponseDto>
            {
                success = false,
                message = message,
                data = new VisorPdfPermissionsResponseDto(),
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

        private static AppResponses<VisorPdfPermissionsResponseDto> ErrorPermissions(string field, string details)
        {
            return new AppResponses<VisorPdfPermissionsResponseDto>
            {
                success = false,
                message = "Error consultando permisos",
                data = new VisorPdfPermissionsResponseDto(),
                meta = new AppMeta { Status = "error" },
                errors =
                [
                    new AppError
                    {
                        Type = "Exception",
                        Field = field,
                        Message = details
                    }
                ]
            };
        }

        private static AppResponses<SimpleOperationResultDto> ValidationOperation(string field, string message)
        {
            return new AppResponses<SimpleOperationResultDto>
            {
                success = false,
                message = message,
                data = new SimpleOperationResultDto(),
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

        private static AppResponses<SimpleOperationResultDto> ErrorOperation(string field, string details)
        {
            return new AppResponses<SimpleOperationResultDto>
            {
                success = false,
                message = "Error operando overrides",
                data = new SimpleOperationResultDto(),
                meta = new AppMeta { Status = "error" },
                errors =
                [
                    new AppError
                    {
                        Type = "Exception",
                        Field = field,
                        Message = details
                    }
                ]
            };
        }
    }
}
