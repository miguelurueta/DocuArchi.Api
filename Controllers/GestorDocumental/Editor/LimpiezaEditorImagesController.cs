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
    public sealed class LimpiezaEditorImagesController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IServiceLimpiezaEditorImages _service;

        public LimpiezaEditorImagesController(
            IClaimValidationService claimValidationService,
            IServiceLimpiezaEditorImages service)
        {
            _claimValidationService = claimValidationService;
            _service = service;
        }

        [HttpPost("images/cleanup")]
        public async Task<ActionResult<AppResponses<LimpiezaEditorImagesResponseDto?>>> Cleanup([FromBody] LimpiezaEditorImagesRequestDto request)
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

            var result = await _service.LimpiaImagenesHuerfanasAsync(request, validation.ClaimValue);
            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("images/cleanup/dry-run")]
        public async Task<ActionResult<AppResponses<LimpiezaEditorImagesResponseDto?>>> DryRun([FromBody] LimpiezaEditorImagesRequestDto request)
        {
            request ??= new LimpiezaEditorImagesRequestDto();
            request.DryRun = true;
            return await Cleanup(request);
        }

        private static AppResponses<LimpiezaEditorImagesResponseDto?> Validation(string field, string message)
        {
            return new AppResponses<LimpiezaEditorImagesResponseDto?>
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
