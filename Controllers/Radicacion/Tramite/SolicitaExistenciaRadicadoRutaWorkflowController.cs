using MiApp.DTOs.DTOs.Utilidades;
using MiApp.DTOs.DTOs.Workflow.RutaTrabajo;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using MiApp.Services.Service.Workflow.RutaTrabajo;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.Radicacion.Tramite
{
    [Route("api/workflow/ruta-trabajo")]
    [ApiController]
    public sealed class SolicitaExistenciaRadicadoRutaWorkflowController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly ISolicitaExistenciaRadicadoRutaWorkflowService _service;

        public SolicitaExistenciaRadicadoRutaWorkflowController(
            IClaimValidationService claimValidationService,
            ISolicitaExistenciaRadicadoRutaWorkflowService service)
        {
            _claimValidationService = claimValidationService;
            _service = service;
        }

        /// <summary>
        /// Verifica existencia de un radicado en la ruta de workflow indicada.
        /// </summary>
        /// <param name="consecutivoRadicado">Consecutivo de radicado.</param>
        /// <param name="nombreRuta">Nombre/sufijo de la ruta workflow.</param>
        /// <returns>Resultado de existencia con estado YES/NO.</returns>
        [HttpGet("solicita-existencia-radicado")]
        public async Task<ActionResult<AppResponses<SolicitaExistenciaRadicadoRutaWorkflowDto>>> SolicitaExistenciaRadicadoRutaWorkflow(
            string consecutivoRadicado,
            string nombreRuta)
        {
            var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!validation.Success || validation.ClaimValue == null)
            {
                return BadRequest(validation.Response);
            }

            var result = await _service.SolicitaExistenciaRadicadoRutaWorkflowAsync(
                consecutivoRadicado,
                nombreRuta,
                validation.ClaimValue);

            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
