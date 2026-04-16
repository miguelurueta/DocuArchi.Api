using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.GestorDocumental.Editor;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.Editor;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.GestorDocumental.Editor
{
    [Route("api/gestor-documental/editor")]
    [ApiController]
    public sealed class SincronizaEditorDocumentImagesController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IServiceSincronizaEditorDocumentImages _service;

        public SincronizaEditorDocumentImagesController(
            IClaimValidationService claimValidationService,
            IServiceSincronizaEditorDocumentImages service)
        {
            _claimValidationService = claimValidationService;
            _service = service;
        }

        [HttpPost("document/images/sync")]
        public async Task<ActionResult<AppResponses<bool>>> Sync([FromBody] SincronizaEditorDocumentImagesRequestDto request)
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
                return BadRequest(Validation("documentId", "DocumentId requerido"));
            }

            if (request.ImageUids == null)
            {
                return BadRequest(Validation("imageUids", "ImageUids requerido"));
            }

            var result = await _service.SincronizaAsync(request, validation.ClaimValue);
            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        private static AppResponses<bool> Validation(string field, string message)
        {
            return new AppResponses<bool>
            {
                success = false,
                message = message,
                data = false,
                errors =
                [
                    new AppError { Type = "Validation", Field = field, Message = message }
                ]
            };
        }
    }
}
