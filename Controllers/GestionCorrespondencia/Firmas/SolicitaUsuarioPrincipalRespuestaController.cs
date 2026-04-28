using MiApp.DTOs.DTOs.Common;
using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestionCorrespondencia.Firmas;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.GestionCorrespondencia.Firmas
{
    [Authorize]
    [Route("api/gestion-correspondencia/firmas")]
    [ApiController]
    public sealed class SolicitaUsuarioPrincipalRespuestaController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IServiceSolicitaUsuarioPrincipalRespuesta _service;
        private readonly ILogger<SolicitaUsuarioPrincipalRespuestaController> _logger;

        public SolicitaUsuarioPrincipalRespuestaController(
            IClaimValidationService claimValidationService,
            IServiceSolicitaUsuarioPrincipalRespuesta service,
            ILogger<SolicitaUsuarioPrincipalRespuestaController> logger)
        {
            _claimValidationService = claimValidationService;
            _service = service;
            _logger = logger;
        }

        [HttpGet("usuario-principal-respuesta")]
        public async Task<ActionResult<AppResponses<ResponseDropdownDto?>>> Get([FromQuery] int idUsuarioGestion)
        {
            var aliasValidation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!aliasValidation.Success || aliasValidation.ClaimValue == null)
            {
                return BadRequest(aliasValidation.Response);
            }

            var usuarioValidation = _claimValidationService.ValidateClaim<string>("usuarioid");
            if (!usuarioValidation.Success || usuarioValidation.ClaimValue == null)
            {
                return BadRequest(usuarioValidation.Response);
            }

            if (!int.TryParse(usuarioValidation.ClaimValue, out var usuarioId) || usuarioId <= 0)
            {
                return BadRequest(Validation("usuarioid", "Claim invalido: usuarioid"));
            }

            if (idUsuarioGestion <= 0)
            {
                return BadRequest(Validation("idUsuarioGestion", "IdUsuarioGestion requerido"));
            }

            var requestId = HttpContext.TraceIdentifier;
            var xRequestId = Request.Headers["X-Request-Id"].ToString();
            _logger.LogInformation(
                "SolicitaUsuarioPrincipalRespuesta: idUsuarioGestion={IdUsuarioGestion} usuarioId={UsuarioId} alias={Alias} requestId={RequestId} xRequestId={XRequestId}",
                idUsuarioGestion,
                usuarioId,
                aliasValidation.ClaimValue,
                requestId,
                xRequestId);

            var result = await _service.SolicitaUsuarioPrincipalRespuestaAsync(
                idUsuarioGestion,
                usuarioId,
                aliasValidation.ClaimValue);

            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        private static AppResponses<ResponseDropdownDto?> Validation(string field, string message)
        {
            return new AppResponses<ResponseDropdownDto?>
            {
                success = false,
                message = message,
                data = null,
                meta = new AppMeta { Status = "error" },
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
