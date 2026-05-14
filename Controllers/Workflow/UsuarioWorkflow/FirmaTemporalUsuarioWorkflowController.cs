using MiApp.DTOs.DTOs.Workflow.Usuario;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using MiApp.Services.Service.Workflow.Usuario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.Workflow.UsuarioWorkflow
{
    [Authorize]
    [Route("api/workflow/usuarios")]
    [ApiController]
    public sealed class FirmaTemporalUsuarioWorkflowController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IServiceFirmaTemporalUsuarioWorkflow _service;

        public FirmaTemporalUsuarioWorkflowController(
            IClaimValidationService claimValidationService,
            IServiceFirmaTemporalUsuarioWorkflow service)
        {
            _claimValidationService = claimValidationService;
            _service = service;
        }

        [HttpGet("firma-temporal")]
        public async Task<ActionResult<AppResponses<FirmaTemporalUsuarioWorkflowDto?>>> Get()
        {
            var aliasValidation = _claimValidationService.ValidateClaim<string>("defaulaliaswf");
            if (!aliasValidation.Success || aliasValidation.ClaimValue == null)
            {
                return BadRequest(aliasValidation.Response);
            }

            var idValidation = _claimValidationService.ValidateClaim<string>("IdUsuarioWorkflow");
            if (!idValidation.Success || idValidation.ClaimValue == null)
            {
                return BadRequest(idValidation.Response);
            }

            if (!int.TryParse(idValidation.ClaimValue, out var idUsuarioWorkflow) || idUsuarioWorkflow <= 0)
            {
                return BadRequest(Validation("IdUsuarioWorkflow", "Claim invalido: IdUsuarioWorkflow"));
            }

            var result = await _service.SolicitaFirmaTemporalAsync(idUsuarioWorkflow, aliasValidation.ClaimValue);
            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("firma-temporal/download/{token}")]
        public ActionResult Download(string token)
        {
            var idValidation = _claimValidationService.ValidateClaim<string>("IdUsuarioWorkflow");
            if (!idValidation.Success || idValidation.ClaimValue == null)
            {
                return BadRequest(idValidation.Response);
            }

            if (!int.TryParse(idValidation.ClaimValue, out var idUsuarioWorkflow) || idUsuarioWorkflow <= 0)
            {
                return BadRequest(Validation("IdUsuarioWorkflow", "Claim invalido: IdUsuarioWorkflow"));
            }

            var found = _service.TryResolveFirmaTemporal(
                token,
                idUsuarioWorkflow,
                out var filePath,
                out var contentType,
                out var fileName);

            if (!found)
            {
                return NotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, contentType, fileName);
        }

        private static AppResponses<FirmaTemporalUsuarioWorkflowDto?> Validation(string field, string message)
        {
            return new AppResponses<FirmaTemporalUsuarioWorkflowDto?>
            {
                success = false,
                message = message,
                data = null,
                meta = new AppMeta { Status = "validation" },
                errors =
                [
                    new MiApp.DTOs.DTOs.Errors.AppError
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
