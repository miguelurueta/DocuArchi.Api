using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.GestorDocumental.Editor;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.Editor;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace DocuArchi.Api.Controllers.GestorDocumental.Editor
{
    [Authorize]
    [ApiController]
    [Route("api/gestor-documental/editor")]
    public sealed class ResolveEditorDocumentController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IServiceResolveEditorDocument _service;

        public ResolveEditorDocumentController(
            IClaimValidationService claimValidationService,
            IServiceResolveEditorDocument service)
        {
            _claimValidationService = claimValidationService;
            _service = service;
        }

        [HttpGet("document/resolve")]
        public async Task<ActionResult<AppResponses<EditorResolveDocumentResponseDto?>>> Resolve(
            [FromQuery] string contextCode,
            [FromQuery] long entityId,
            [FromQuery] long idTareaWf = 0,
            [FromQuery] long? templateDefinitionId = null,
            [FromQuery] string? templateCode = null,
            [FromQuery] string? prefer = null)
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

            if (!string.IsNullOrWhiteSpace(prefer))
            {
                var p = prefer.Trim().ToLowerInvariant();
                if (p != "existing" && p != "initial")
                {
                    return BadRequest(Validation("prefer", "Prefer inválido. Valores permitidos: existing|initial"));
                }
            }

            var result = await _service.ResolveAsync(idTareaWf, contextCode, entityId, templateDefinitionId, templateCode, prefer, validation.ClaimValue);
            if (result == null)
            {
                return BadRequest(Validation("contextCode", "Sin respuesta del servicio"));
            }

            if (!result.success)
            {
                var hasConflict = result.errors != null
                    && result.errors.OfType<AppError>().Any(e => e.Type == "Conflict");

                if (hasConflict)
                {
                    return Conflict(result);
                }

                return BadRequest(result);
            }

            return Ok(result);
        }

        private static AppResponses<EditorResolveDocumentResponseDto?> Validation(string field, string message)
        {
            return new AppResponses<EditorResolveDocumentResponseDto?>
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