using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.GestionCorrespondencia.GestionRespuesta.Core;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestionCorrespondencia.GestionRespuesta.Core;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.GestionCorrespondencia.GestionRespuesta
{
    [Authorize]
    [Route("api/gestion-correspondencia/respuestas")]
    [ApiController]
    public sealed class DocumentosAdjuntosRespuestaController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IServiceSolicitaDocumentosAdjuntosRespuestaRadicado _service;
        private readonly ILogger<DocumentosAdjuntosRespuestaController> _logger;

        public DocumentosAdjuntosRespuestaController(
            IClaimValidationService claimValidationService,
            IServiceSolicitaDocumentosAdjuntosRespuestaRadicado service,
            ILogger<DocumentosAdjuntosRespuestaController> logger)
        {
            _claimValidationService = claimValidationService;
            _service = service;
            _logger = logger;
        }

        [HttpGet("{idRespuestaRadicado:long}/documentos-adjuntos")]
        public async Task<ActionResult<AppResponses<DocumentoAdjuntoRespuestaRadicadoListDto>>> Get(
            [FromRoute] long idRespuestaRadicado,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 25,
            [FromQuery] string sortField = "idAnexoRespuesta",
            [FromQuery] string sortDir = "DESC",
            [FromQuery] bool includeTotal = true)
        {
            var aliasValidation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!aliasValidation.Success || string.IsNullOrWhiteSpace(aliasValidation.ClaimValue))
            {
                return BadRequest(aliasValidation.Response);
            }

            var usuarioValidation = _claimValidationService.ValidateClaim<string>("usuarioid");
            if (!usuarioValidation.Success || string.IsNullOrWhiteSpace(usuarioValidation.ClaimValue))
            {
                return BadRequest(usuarioValidation.Response);
            }

            if (!int.TryParse(usuarioValidation.ClaimValue, out var usuarioId) || usuarioId <= 0)
            {
                return BadRequest(Validation("usuarioid", "Claim invalido: usuarioid", idRespuestaRadicado));
            }

            if (idRespuestaRadicado <= 0)
            {
                return BadRequest(Validation("idRespuestaRadicado", "IdRespuestaRadicado requerido", idRespuestaRadicado));
            }

            var request = new DocumentoAdjuntoRespuestaRadicadoQueryDto
            {
                Page = page,
                PageSize = pageSize,
                SortField = sortField,
                SortDir = sortDir,
                IncludeTotal = includeTotal
            };

            _logger.LogInformation(
                "DocumentosAdjuntosRespuesta: idRespuestaRadicado={IdRespuestaRadicado} usuarioId={UsuarioId} alias={Alias} page={Page} pageSize={PageSize}",
                idRespuestaRadicado,
                usuarioId,
                aliasValidation.ClaimValue,
                page,
                pageSize);

            var result = await _service.SolicitaDocumentosAdjuntosRespuestaRadicadoAsync(
                idRespuestaRadicado,
                usuarioId,
                aliasValidation.ClaimValue.Trim(),
                request);

            if (!result.success && string.Equals(result.meta?.Status, "forbidden", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status403Forbidden, result);
            }

            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        private static AppResponses<DocumentoAdjuntoRespuestaRadicadoListDto> Validation(
            string field,
            string message,
            long idRespuestaRadicado)
        {
            return new AppResponses<DocumentoAdjuntoRespuestaRadicadoListDto>
            {
                success = false,
                message = message,
                data = new DocumentoAdjuntoRespuestaRadicadoListDto { IdRespuestaRadicado = idRespuestaRadicado },
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
