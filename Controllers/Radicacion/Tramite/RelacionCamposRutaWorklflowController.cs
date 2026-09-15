using MiApp.DTOs.DTOs.Radicacion.Tramite;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.Radicacion.Tramite;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.Radicacion.RelacionCamposRutaWorklflow
{
    [Route("api/radicacion")]
    [ApiController]
    public sealed class RelacionCamposRutaWorklflowController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IRelacionCamposRutaWorklflowService _service;

        public RelacionCamposRutaWorklflowController(
            IClaimValidationService claimValidationService,
            IRelacionCamposRutaWorklflowService service)
        {
            _claimValidationService = claimValidationService;
            _service = service;
        }

        /// <summary>
        /// Consulta campos relacionados entre plantilla de radicacion y ruta workflow.
        /// </summary>
        /// <param name="idPlantillaRadicado">Id de plantilla de radicacion.</param>
        /// <param name="idRuta">Id de ruta workflow.</param>
        /// <returns>AppResponses con los campos relacionados.</returns>
        [HttpGet("tramite/solicita-campos-relacion-ruta-plantilla")]
        public async Task<ActionResult<AppResponses<List<RelacionCamposRutaWorklflowDto>>>> SolicitaCamposRelacionRutaPlantilla(
            int idPlantillaRadicado,
            int idRuta)
        {
            var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!validation.Success || validation.ClaimValue == null)
            {
                return BadRequest(validation.Response);
            }

            var result = await _service.SolicitaCamposRelacionRutaPlantillaAsync(
                idPlantillaRadicado,
                idRuta,
                validation.ClaimValue);

            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
