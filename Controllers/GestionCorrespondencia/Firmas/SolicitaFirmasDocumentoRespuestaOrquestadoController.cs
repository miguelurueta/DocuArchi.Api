using MiApp.DTOs.DTOs.Common;
using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestionCorrespondencia.Firmas;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.GestionCorrespondencia.Firmas
{
    //[Authorize]
    [Route("api/gestion-correspondencia/firmas")]
    [ApiController]
    public sealed class SolicitaFirmasDocumentoRespuestaOrquestadoController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IServiceSolicitaFirmasDocumentoRespuestaOrquestado _service;

        public SolicitaFirmasDocumentoRespuestaOrquestadoController(
            IClaimValidationService claimValidationService,
            IServiceSolicitaFirmasDocumentoRespuestaOrquestado service)
        {
            _claimValidationService = claimValidationService;
            _service = service;
        }

        [HttpGet("documento-respuesta-orquestado")]
        public async Task<ActionResult<AppResponses<List<ResponseDropdownDto>>>> Get(
            [FromQuery] int idUsuarioGestion,
            [FromQuery] long idSolicitudAprobacion)
        {
            //var aliasValidation = _claimValidationService.ValidateClaim<string>("defaulalias");
            //if (!aliasValidation.Success || string.IsNullOrWhiteSpace(aliasValidation.ClaimValue))
            //{
            //    return BadRequest(aliasValidation.Response);
            //}

            //var usuarioValidation = _claimValidationService.ValidateClaim<string>("usuarioid");
            //if (!usuarioValidation.Success || string.IsNullOrWhiteSpace(usuarioValidation.ClaimValue))
            //{
            //    return BadRequest(usuarioValidation.Response);
            //}

            //if (!int.TryParse(usuarioValidation.ClaimValue, out var usuarioId) || usuarioId <= 0)
            //{
            //    return BadRequest(Validation("usuarioid", "Claim invalido: usuarioid"));
            //}

            //if (idUsuarioGestion <= 0)
            //{
            //    return BadRequest(Validation("idUsuarioGestion", "IdUsuarioGestion requerido"));
            //}

            //if (idSolicitudAprobacion <= 0)
            //{
            //    return BadRequest(Validation("idSolicitudAprobacion", "IdSolicitudAprobacion requerido"));
            //}

            //var result = await _service.SolicitaFirmasDocumentoRespuestaOrquestadoAsync(
            //    idUsuarioGestion,
            //    idSolicitudAprobacion,
            //    usuarioId,
            //    aliasValidation.ClaimValue.Trim());

            var result = await _service.SolicitaFirmasDocumentoRespuestaOrquestadoAsync(
                idUsuarioGestion,
                idSolicitudAprobacion,
                136,
                "DA");

            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        private static AppResponses<List<ResponseDropdownDto>> Validation(string field, string message)
        {
            return new AppResponses<List<ResponseDropdownDto>>
            {
                success = false,
                message = message,
                data = [],
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
