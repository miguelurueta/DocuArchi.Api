using System.Security;
using MiApp.DTOs.DTOs.UI.MuiTable;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.DTOs.DTOs.Workflow.BandejaCorrespondencia;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using MiApp.Services.Service.Workflow.BandejaCorrespondencia;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.WorkflowInboxGestion
{
    [Route("api/workflowInboxgestion")]
    [ApiController]
    [Authorize]
    public sealed class WorkflowInboxController : Controller
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IWorkflowInboxService _workflowInboxService;

        public WorkflowInboxController(
            IClaimValidationService claimValidationService,
            IWorkflowInboxService workflowInboxService)
        {
            _claimValidationService = claimValidationService;
            _workflowInboxService = workflowInboxService;
        }

        [HttpPost("inboxgestion")]
        public async Task<ActionResult<AppResponses<DynamicUiTableDto>>> SolicitaBandejaWorkflow(
            [FromBody] WorkflowInboxApiRequestDto request)
        {
            var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!validation.Success || validation.ClaimValue == null)
            {
                return BadRequest(validation.Response);
            }

            var validationUsuario = _claimValidationService.ValidateClaim<string>("usuarioid");
            if (!validationUsuario.Success || validationUsuario.ClaimValue == null)
            {
                return BadRequest(validationUsuario.Response);
            }

            if (!int.TryParse(validationUsuario.ClaimValue, out var idUsuarioGestion))
            {
                throw new SecurityException("Claim invalido: usuarioid");
            }

            var result = await _workflowInboxService.SolicitaBandejaWorkflowAsync(
                request,
                idUsuarioGestion,
                validation.ClaimValue);
            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("/api/AppTable/export")]
        public async Task<ActionResult> ExportaBandejaWorkflow(
            [FromBody] WorkflowInboxExportRequestDto request)
        {
            var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!validation.Success || validation.ClaimValue == null)
            {
                return BadRequest(validation.Response);
            }

            var validationUsuario = _claimValidationService.ValidateClaim<string>("usuarioid");
            if (!validationUsuario.Success || validationUsuario.ClaimValue == null)
            {
                return BadRequest(validationUsuario.Response);
            }

            if (!int.TryParse(validationUsuario.ClaimValue, out var idUsuarioGestion))
            {
                throw new SecurityException("Claim invalido: usuarioid");
            }

            var result = await _workflowInboxService.ExportBandejaWorkflowAsync(
                request,
                idUsuarioGestion,
                validation.ClaimValue);

            if (!result.success || result.data == null)
            {
                return BadRequest(result);
            }

            return File(
                result.data.FileBytes,
                result.data.ContentType,
                result.data.FileName);
        }
    }
}
