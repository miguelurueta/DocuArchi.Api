using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.GestionCorrespondencia.GestionRespuesta;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestionCorrespondencia.GestionRespuesta;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DocuArchi.Api.Controllers.GestionCorrespondencia.GestionRespuesta
{
    [Authorize]
    [Route("api/GestionCorrespondencia")]
    [ApiController]
    public sealed class SolicitaDocumentosAdjuntosRespuestaRadicadoController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IServiceSolicitaDocumentosAdjuntosRespuestaRadicado _service;
        private readonly ILogger<SolicitaDocumentosAdjuntosRespuestaRadicadoController> _logger;

        public SolicitaDocumentosAdjuntosRespuestaRadicadoController(
            IClaimValidationService claimValidationService,
            IServiceSolicitaDocumentosAdjuntosRespuestaRadicado service,
            ILogger<SolicitaDocumentosAdjuntosRespuestaRadicadoController> logger)
        {
            _claimValidationService = claimValidationService;
            _service = service;
            _logger = logger;
        }

        [HttpGet("solicita-documentos-adjuntos-respuesta-radicado")]
        public async Task<ActionResult<AppResponses<List<DocumentoAdjuntoRespuestaRadicadoDto>>>> Get([FromQuery] long idRespuestaRadicado)
        {
            var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!validation.Success || string.IsNullOrWhiteSpace(validation.ClaimValue))
            {
                return BadRequest(validation.Response);
            }

            if (idRespuestaRadicado <= 0)
            {
                return BadRequest(new AppResponses<List<DocumentoAdjuntoRespuestaRadicadoDto>>
                {
                    success = false,
                    message = "IdRespuestaRadicado requerido",
                    data = [],
                    errors =
                    [
                        new AppError
                        {
                            Type = "Validation",
                            Field = "idRespuestaRadicado",
                            Message = "IdRespuestaRadicado requerido"
                        }
                    ]
                });
            }

            _logger.LogInformation(
                "SolicitaDocumentosAdjuntosRespuestaRadicado: idRespuestaRadicado={IdRespuestaRadicado} alias={Alias}",
                idRespuestaRadicado,
                validation.ClaimValue);

            var result = await _service.SolicitaDocumentosAdjuntosRespuestaRadicadoAsync(idRespuestaRadicado, validation.ClaimValue.Trim());
            if (!result.success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
