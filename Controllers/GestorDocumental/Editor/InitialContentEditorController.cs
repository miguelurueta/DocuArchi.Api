using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.GestorDocumental.Editor;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.Editor;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DocuArchi.Api.Controllers.GestorDocumental.Editor
{
    //[Authorize]
    [ApiController]
    [Route("api/gestor-documental/editor")]
    public sealed class InitialContentEditorController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IServiceInitialContentEditor _service;

        public InitialContentEditorController(
            IClaimValidationService claimValidationService,
            IServiceInitialContentEditor service)
        {
            _claimValidationService = claimValidationService;
            _service = service;
        }

        [HttpGet("initial-content")]
        public async Task<ActionResult<AppResponses<EditorInitialContentResponseDto?>>> GetInitialContent(
            [FromQuery] long idTareaWf,
            [FromQuery] string contextCode,
            [FromQuery] long entityId,
            [FromQuery] long? templateDefinitionId = null,
            [FromQuery] string? templateCode = null)
        {
            //var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
            //if (!validation.Success || validation.ClaimValue == null)
            //{
            //    return BadRequest(validation.Response);
            //}

            if (idTareaWf <= 0)
            {
                return BadRequest(Validation("idTareaWf", "IdTareaWf requerido"));
            }

            if (string.IsNullOrWhiteSpace(contextCode))
            {
                return BadRequest(Validation("contextCode", "ContextCode requerido"));
            }

            if (entityId <= 0)
            {
                return BadRequest(Validation("entityId", "EntityId requerido"));
            }

            var result = await _service.GetInitialContentAsync(idTareaWf, contextCode, entityId, "DA", templateDefinitionId, templateCode);
            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        private static AppResponses<EditorInitialContentResponseDto?> Validation(string field, string message)
        {
            return new AppResponses<EditorInitialContentResponseDto?>
            {
                success = false,
                message = message,
                data = null,
                errors =
                [
                    new AppError { Type = "Validation", Field = field, Message = message }
                ]
            };
        }
    }
}
