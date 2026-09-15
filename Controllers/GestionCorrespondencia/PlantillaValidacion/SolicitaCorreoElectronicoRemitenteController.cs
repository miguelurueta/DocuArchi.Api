using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestionCorrespondencia.PlantillaValidacion.SolicitaCorreoElectronicoRemitente;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DocuArchi.Api.Controllers.GestionCorrespondencia.PlantillaValidacion
{
    [Authorize]
    [Route("api/GestionCorrespondencia/PlantillaValidacion")]
    [ApiController]
    public sealed class SolicitaCorreoElectronicoRemitenteController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IServiceSolicitaCorreoElectronicoRemitente _service;
        private readonly ILogger<SolicitaCorreoElectronicoRemitenteController> _logger;

        public SolicitaCorreoElectronicoRemitenteController(
            IClaimValidationService claimValidationService,
            IServiceSolicitaCorreoElectronicoRemitente service,
            ILogger<SolicitaCorreoElectronicoRemitenteController> logger)
        {
            _claimValidationService = claimValidationService;
            _service = service;
            _logger = logger;
        }

        [HttpGet("solicita-correo-electronico-remitente")]
        public async Task<ActionResult<AppResponses<string>>> Get(
            [FromQuery] long idPlantillaRadicado,
            [FromQuery] long idDestinatarioExterno)
        {
            var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!validation.Success || validation.ClaimValue == null)
            {
                return BadRequest(validation.Response);
            }

            if (idPlantillaRadicado <= 0)
            {
                return BadRequest(Validation("idPlantillaRadicado", "IdPlantillaRadicado requerido"));
            }

            if (idDestinatarioExterno <= 0)
            {
                return BadRequest(Validation("idDestinatarioExterno", "IdDestinatarioExterno requerido"));
            }

            var requestId = HttpContext.TraceIdentifier;
            var xRequestId = Request.Headers["X-Request-Id"].ToString();

            _logger.LogInformation(
                "SolicitaCorreoElectronicoRemitente: idPlantillaRadicado={IdPlantillaRadicado} idDestinatarioExterno={IdDestinatarioExterno} alias={Alias} requestId={RequestId} xRequestId={XRequestId}",
                idPlantillaRadicado,
                idDestinatarioExterno,
                validation.ClaimValue,
                requestId,
                xRequestId
            );

            var result = await _service.SolicitaCorreoElectronicoRemitenteAsync(
                idPlantillaRadicado,
                idDestinatarioExterno,
                validation.ClaimValue);

            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        private static AppResponses<string> Validation(string field, string message)
        {
            return new AppResponses<string>
            {
                success = false,
                message = message,
                data = string.Empty,
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
