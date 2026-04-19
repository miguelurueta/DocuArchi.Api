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
    [Authorize]
    [ApiController]
    [Route("api/gestor-documental/editor")]
    public sealed class SolicitaEditorDocumentByContextController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IServiceSolicitaEditorDocumentByContext _service;

        public SolicitaEditorDocumentByContextController(
            IClaimValidationService claimValidationService,
            IServiceSolicitaEditorDocumentByContext service)
        {
            _claimValidationService = claimValidationService;
            _service = service;
        }

        [HttpGet("document/by-context")]
        public async Task<ActionResult<AppResponses<EditorDocumentDetailByContextResponseDto?>>> GetByContext(
            [FromQuery] string contextCode,
            [FromQuery] long entityId)
        {
            var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!validation.Success || validation.ClaimValue == null)
            {
                return BadRequest(validation.Response);
            }

            if (string.IsNullOrWhiteSpace(contextCode))
            {
                return BadRequest(Validation("contextCode", "ContextCode requerido"));
            }

            if (entityId <= 0)
            {
                return BadRequest(Validation("entityId", "EntityId requerido"));
            }

            var result = await _service.SolicitaEditorDocumentByContextAsync(contextCode, entityId, validation.ClaimValue);
            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        private static AppResponses<EditorDocumentDetailByContextResponseDto?> Validation(string field, string message)
        {
            return new AppResponses<EditorDocumentDetailByContextResponseDto?>
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
