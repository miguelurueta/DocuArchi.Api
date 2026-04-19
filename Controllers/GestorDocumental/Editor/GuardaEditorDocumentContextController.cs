using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.GestorDocumental.Editor;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Models.Models.GestorDocumental.Editor;
using MiApp.Services.Service.GestorDocumental.Editor;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.GestorDocumental.Editor
{
    [Route("api/gestor-documental/editor")]
    [ApiController]
    public sealed class GuardaEditorDocumentContextController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IServiceGuardaEditorDocumentContext _service;

        public GuardaEditorDocumentContextController(
            IClaimValidationService claimValidationService,
            IServiceGuardaEditorDocumentContext service)
        {
            _claimValidationService = claimValidationService;
            _service = service;
        }

        [HttpPost("document/context")]
        public async Task<ActionResult<AppResponses<RaEditorDocumentContext?>>> GuardaContext([FromBody] GuardaEditorDocumentContextDto request)
        {
            var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!validation.Success || validation.ClaimValue == null)
            {
                return BadRequest(validation.Response);
            }

            if (request == null)
            {
                return BadRequest(Validation("request", "Request requerido"));
            }

            if (request.DocumentId <= 0)
            {
                return BadRequest(Validation("DocumentId", "DocumentId requerido"));
            }

            if (string.IsNullOrWhiteSpace(request.ContextCode))
            {
                return BadRequest(Validation("ContextCode", "ContextCode requerido"));
            }

            if (request.EntityId <= 0)
            {
                return BadRequest(Validation("EntityId", "EntityId requerido"));
            }

            var result = await _service.GuardaEditorDocumentContextAsync(request, validation.ClaimValue);
            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        private static AppResponses<RaEditorDocumentContext?> Validation(string field, string message)
        {
            return new AppResponses<RaEditorDocumentContext?>
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
