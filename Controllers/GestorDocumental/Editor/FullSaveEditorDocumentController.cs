using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiApp.DTOs.DTOs.GestorDocumental.Editor;
using MiApp.Models.Models.GestorDocumental.Editor;
using MiApp.Services.Service.GestorDocumental.Editor;
using MiApp.Services.Service.Seguridad;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using System.Threading.Tasks;

namespace DocuArchi.Api.Controllers.GestorDocumental.Editor
{
    [Authorize]
    [ApiController]
    [Route("api/gestor-documental/editor")]
    public class FullSaveEditorDocumentController : ControllerBase
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
        public async Task<IActionResult> FullSave([FromBody] FullSaveEditorDocumentRequestDto request)
        {
            var claimResult = _claimValidationService.ValidateClaim<RaEditorDocument>("defaulalias");
            if (!claimResult.Success)
            {
                return BadRequest(claimResult.Response);
            }

            var defaultDbAlias = claimResult.ClaimValue!;
            var result = await _fullSaveService.FullSaveAsync(request, defaultDbAlias);

            if (result == null || !result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
