using MiApp.DTOs.DTOs.Utilidades;
using MiApp.DTOs.DTOs.Workflow.RutaTrabajo;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using MiApp.Services.Service.Workflow.RutaTrabajo;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.Radicacion.Tramite
{
    [Route("api/workflow/ruta-trabajo")]
    [ApiController]
    public sealed class SolicitaEstructuraRutaWorkflowController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly ISolicitaEstructuraRutaWorkflowService _service;

        public SolicitaEstructuraRutaWorkflowController(
            IClaimValidationService claimValidationService,
            ISolicitaEstructuraRutaWorkflowService service)
        {
            _claimValidationService = claimValidationService;
            _service = service;
        }

        /// <summary>
        /// Solicita la estructura de las rutas workflow activas.
        /// </summary>
        /// <returns>Resultado con la lista de rutas activas o sin resultados.</returns>
        [HttpGet("solicita-estructura-ruta")]
        public async Task<ActionResult<AppResponses<List<SolicitaEstructuraRutaWorkflowDto>?>>> SolicitaEstructuraRutaWorkflow()
        {
            var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!validation.Success || validation.ClaimValue == null)
            {
                return BadRequest(validation.Response);
            }

            var result = await _service.SolicitaEstructuraRutaWorkflowAsync(validation.ClaimValue);
            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
