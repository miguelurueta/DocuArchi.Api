using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Models.Models.GestionCorrespondencia;
using MiApp.Services.Service.GestorDocumental;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySqlX.XDevAPI.Common;

namespace DocuArchi.Api.Controllers.GestionCorrespondencia
{
    [Authorize]
    [Route("api/GestionCorrespondencia")]
    [ApiController]
    public sealed class SolicitaEstructuraRespuestaIdTareaController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IServiceSolicitaEstructuraRespuesta _service;
        private readonly ILogger<SolicitaEstructuraRespuestaIdTareaController> _logger;

        public SolicitaEstructuraRespuestaIdTareaController(
    IClaimValidationService claimValidationService,
    IServiceSolicitaEstructuraRespuesta service,
    ILogger<SolicitaEstructuraRespuestaIdTareaController> logger)
{
    _claimValidationService = claimValidationService;
    _service = service;
    _logger = logger;
}

        [HttpGet("solicita-estructura-respuesta-id-tarea")]
        public async Task<ActionResult<AppResponses<List<RaRespuestaRadicado>>>> SolicitaEstructuraRespuestaIdTarea([FromQuery] long idTareaWf)
        {
            var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!validation.Success || validation.ClaimValue == null)
            {
                return BadRequest(validation.Response);
            }

            if (idTareaWf <= 0)
            {
                return BadRequest(new AppResponses<List<RaRespuestaRadicado>>
                {
                    success = false,
                    message = "IdTareaWf requerido",
                    data = [],
                    errors =
                    [
                        new AppError
                        {
                            Type = "Validation",
                            Field = "idTareaWf",
                            Message = "IdTareaWf requerido"
                        }
                    ]
                });
            }
            
            var requestId = HttpContext.TraceIdentifier;
            var xRequestId = Request.Headers["X-Request-Id"].ToString();
            _logger.LogInformation(
                "SolicitaEstructuraRespuestaIdTarea: idTareaWf={IdTareaWf} alias={Alias} requestId={RequestId} xRequestId={XRequestId}",
                idTareaWf,
                validation.ClaimValue,
                requestId,
                xRequestId
            );

            var result = await _service.SolicitaEstructuraRespuestaIdTareaAsync(idTareaWf, validation.ClaimValue);
            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}

