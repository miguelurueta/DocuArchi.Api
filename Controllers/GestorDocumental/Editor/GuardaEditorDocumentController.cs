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
    public sealed class GuardaEditorDocumentController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IServiceGuardaEditorDocument _service;

        public GuardaEditorDocumentController(
            IClaimValidationService claimValidationService,
            IServiceGuardaEditorDocument service)
        {
            _claimValidationService = claimValidationService;
            _service = service;
        }

        [HttpPost("guardar-documento")]
        public async Task<ActionResult<AppResponses<RaEditorDocument?>>> GuardarDocumento([FromBody] GuardaEditorDocumentRequestDto request)
        {
            var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!validation.Success || validation.ClaimValue == null)
            {
                return BadRequest(validation.Response);
            }

            if (request == null)
            {
                return BadRequest(new AppResponses<RaEditorDocument?>
                {
                    success = false,
                    message = "Request requerido",
                    data = null,
                    errors =
                    [
                        new AppError
                        {
                            Type = "Validation",
                            Field = "request",
                            Message = "Request requerido"
                        }
                    ]
                });
            }

            if (string.IsNullOrWhiteSpace(request.DocumentHtml))
            {
                return BadRequest(new AppResponses<RaEditorDocument?>
                {
                    success = false,
                    message = "DocumentHtml requerido",
                    data = null,
                    errors =
                    [
                        new AppError
                        {
                            Type = "Validation",
                            Field = "documentHtml",
                            Message = "DocumentHtml requerido"
                        }
                    ]
                });
            }

            var result = await _service.GuardaEditorDocumentAsync(request, validation.ClaimValue);
            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
