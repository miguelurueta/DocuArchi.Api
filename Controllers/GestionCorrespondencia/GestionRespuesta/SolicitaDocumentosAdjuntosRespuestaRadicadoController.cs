using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.GestionCorrespondencia.GestionRespuesta.Core;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestionCorrespondencia.GestionRespuesta.Core;
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

            var usuarioValidation = _claimValidationService.ValidateClaim<string>("usuarioid");
            if (!usuarioValidation.Success || string.IsNullOrWhiteSpace(usuarioValidation.ClaimValue))
            {
                return BadRequest(usuarioValidation.Response);
            }

            if (!int.TryParse(usuarioValidation.ClaimValue, out var usuarioId) || usuarioId <= 0)
            {
                return BadRequest(Validation("usuarioid", "Claim invalido: usuarioid"));
            }

            if (idRespuestaRadicado <= 0)
            {
                return BadRequest(Validation("idRespuestaRadicado", "IdRespuestaRadicado requerido"));
            }

            _logger.LogInformation(
                "SolicitaDocumentosAdjuntosRespuestaRadicado: idRespuestaRadicado={IdRespuestaRadicado} usuarioId={UsuarioId} alias={Alias}",
                idRespuestaRadicado,
                usuarioId,
                validation.ClaimValue);

            var result = await _service.SolicitaDocumentosAdjuntosRespuestaRadicadoAsync(idRespuestaRadicado, usuarioId, validation.ClaimValue.Trim());
            if (!result.success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        private static AppResponses<List<DocumentoAdjuntoRespuestaRadicadoDto>> Validation(string field, string message)
        {
            return new AppResponses<List<DocumentoAdjuntoRespuestaRadicadoDto>>
            {
                success = false,
                message = message,
                data = [],
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
