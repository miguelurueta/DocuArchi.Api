using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.DTOs.DTOs.Workflow.RutaTrabajo;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using MiApp.Services.Service.Workflow.RutaTrabajo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.Radicacion.Tramite
{
    [Route("api/workflow/ruta-trabajo")]
    [ApiController]
    //[Authorize]
    public sealed class SolicitaGabineteRadicadoWorkflowController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly ISolicitaGabineteRadicadoWorkflowService _service;

        public SolicitaGabineteRadicadoWorkflowController(
            IClaimValidationService claimValidationService,
            ISolicitaGabineteRadicadoWorkflowService service)
        {
            _claimValidationService = claimValidationService;
            _service = service;
        }

        /// <summary>
        /// Consulta metadatos de gabinete workflow por consecutivo de radicado.
        /// </summary>
        [HttpGet("radicados/{consecutivoRadicado}/gabinete")]
        public async Task<ActionResult<AppResponses<RadicadoGabineteWorkflowDto>>> SolicitaPorRadicado(string consecutivoRadicado)
        {
            try
            {
                var workflowAliasValidation = _claimValidationService.ValidateClaim<string>("defaulaliaswf");
                if (!workflowAliasValidation.Success || workflowAliasValidation.ClaimValue == null)
                {
                    return BadRequest(workflowAliasValidation.Response);
                }

                var gestionAliasValidation = _claimValidationService.ValidateClaim<string>("defaulalias");
                var gestionAlias = gestionAliasValidation.Success ? gestionAliasValidation.ClaimValue : null;

                var result = await _service.SolicitaPorRadicadoAsync(
                    consecutivoRadicado,
                    workflowAliasValidation.ClaimValue,
                    gestionAlias);
                if (!result.success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new AppResponses<RadicadoGabineteWorkflowDto>
                {
                    success = false,
                    message = "Error al ejecutar SolicitaPorRadicado",
                    data = new RadicadoGabineteWorkflowDto
                    {
                        Radicado = consecutivoRadicado?.Trim() ?? string.Empty,
                        EstadoExistenciaRadicado = "NO"
                    },
                    errors =
                    [
                        new AppError
                        {
                            Type = "Exception",
                            Field = "consecutivoRadicado",
                            Message = ex.Message
                        }
                    ]
                });
            }
        }

        /// <summary>
        /// Consulta metadatos de gabinete workflow por identificador de tarea workflow.
        /// </summary>
        [HttpGet("tareas/{idTareaWorkflow:long}/gabinete")]
        public async Task<ActionResult<AppResponses<RadicadoGabineteWorkflowDto>>> SolicitaPorIdTareaWorkflow(long idTareaWorkflow)
        {
            try
            {
                var workflowAliasValidation = _claimValidationService.ValidateClaim<string>("defaulaliaswf");
                if (!workflowAliasValidation.Success || workflowAliasValidation.ClaimValue == null)
                {
                    return BadRequest(workflowAliasValidation.Response);
                }

                var gestionAliasValidation = _claimValidationService.ValidateClaim<string>("defaulalias");
                var gestionAlias = gestionAliasValidation.Success ? gestionAliasValidation.ClaimValue : null;

                var result = await _service.SolicitaPorIdTareaWorkflowAsync(
                    idTareaWorkflow,
                    workflowAliasValidation.ClaimValue,
                    gestionAlias);
                if (!result.success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new AppResponses<RadicadoGabineteWorkflowDto>
                {
                    success = false,
                    message = "Error al ejecutar SolicitaPorIdTareaWorkflow",
                    data = new RadicadoGabineteWorkflowDto
                    {
                        IdTareaWorkflow = idTareaWorkflow,
                        EstadoExistenciaRadicado = "NO"
                    },
                    errors =
                    [
                        new AppError
                        {
                            Type = "Exception",
                            Field = "idTareaWorkflow",
                            Message = ex.Message
                        }
                    ]
                });
            }
        }
    }
}
