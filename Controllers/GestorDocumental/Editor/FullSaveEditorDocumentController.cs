using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.GestorDocumental.Editor;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Models.Models.GestorDocumental.Editor;
using MiApp.Services.Service.GestorDocumental.Editor;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using System.Threading.Tasks;

namespace DocuArchi.Api.Controllers.GestorDocumental.Editor
{
    [Authorize]
    [ApiController]
    [Route("api/gestor-documental/editor")]
    public sealed class FullSaveEditorDocumentController : ControllerBase
    {
        private readonly IServiceFullSaveEditorDocument _fullSaveService;
        private readonly IClaimValidationService _claimValidationService;

        public FullSaveEditorDocumentController(
            IServiceFullSaveEditorDocument fullSaveService,
            IClaimValidationService claimValidationService)
        {
            _fullSaveService = fullSaveService;
            _claimValidationService = claimValidationService;
        }

        [HttpPost("document/full-save")]
        public async Task<ActionResult<AppResponses<RaEditorDocument?>>> FullSave([FromBody] FullSaveEditorDocumentRequestDto request)
        {
            var claimResult = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!claimResult.Success || claimResult.ClaimValue == null)
            {
                return BadRequest(claimResult.Response);
            }

            if (request == null)
            {
                return BadRequest(Validation("request", "Request requerido"));
            }

            if (string.IsNullOrWhiteSpace(request.DocumentHtml))
            {
                return BadRequest(Validation("DocumentHtml", "Contenido HTML requerido"));
            }

            if (string.IsNullOrWhiteSpace(request.ContextCode))
            {
                return BadRequest(Validation("ContextCode", "ContextCode requerido"));
            }

            if (request.EntityId <= 0)
            {
                return BadRequest(Validation("EntityId", "EntityId requerido"));
            }

            if (request.ImageUids == null)
            {
                return BadRequest(Validation("ImageUids", "ImageUids requerido"));
            }

            var result = await _fullSaveService.FullSaveAsync(request, claimResult.ClaimValue);

            if (result == null || !result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        private static AppResponses<RaEditorDocument?> Validation(string field, string message)
        {
            return new AppResponses<RaEditorDocument?>
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
