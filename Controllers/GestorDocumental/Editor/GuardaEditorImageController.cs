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
    public sealed class GuardaEditorImageController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IServiceGuardaEditorImage _service;

        public GuardaEditorImageController(
            IClaimValidationService claimValidationService,
            IServiceGuardaEditorImage service)
        {
            _claimValidationService = claimValidationService;
            _service = service;
        }

        
        public sealed class GuardaEditorImageForm
        {
            public IFormFile File { get; set; } = default!;
        }


        [HttpPost("guardar-imagen")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<AppResponses<GuardaEditorImageResponseDto?>>> GuardarImagen([FromForm] GuardaEditorImageForm form)
        {
            var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!validation.Success || validation.ClaimValue == null)
            {
                return BadRequest(validation.Response);
            }


            var file = form.File;
            if (file == null || file.Length <= 0)
            {
                return BadRequest(new AppResponses<GuardaEditorImageResponseDto?>
                {
                    success = false,
                    message = "Archivo requerido",
                    data = null,
                    errors =
                    [
                        new AppError
                        {
                            Type = "Validation",
                            Field = "file",
                            Message = "Archivo requerido"
                        }
                    ]
                });
            }

            await using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            var bytes = stream.ToArray();

            var result = await _service.GuardaEditorImageAsync(
                bytes,
                file.FileName,
                file.ContentType,
                validation.ClaimValue);

            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}

