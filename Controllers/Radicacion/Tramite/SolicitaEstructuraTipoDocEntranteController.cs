using MiApp.DTOs.DTOs.Radicacion.Tramite;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.Radicacion.Tramite;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.Radicacion.Tramite
{
    [Route("api/radicacion/tramite")]
    [ApiController]
    public sealed class SolicitaEstructuraTipoDocEntranteController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly ISolicitaEstructuraTipoDocEntranteService _service;

        public SolicitaEstructuraTipoDocEntranteController(
            IClaimValidationService claimValidationService,
            ISolicitaEstructuraTipoDocEntranteService service)
        {
            _claimValidationService = claimValidationService;
            _service = service;
        }

        [HttpGet("tipo-doc-entrante/{idTipoDocEntrante:int}")]
        public async Task<ActionResult<AppResponses<TipoDocEntranteParametroDto>>> SolicitaEstructuraTipoDocEntrante(
            [FromRoute] int idTipoDocEntrante)
        {
            var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!validation.Success || validation.ClaimValue == null)
            {
                return BadRequest(validation.Response);
            }

            var result = await _service.SolicitaEstructuraTipoDocEntranteAsync(idTipoDocEntrante, validation.ClaimValue);
            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
