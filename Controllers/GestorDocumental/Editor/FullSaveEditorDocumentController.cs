using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiApp.DTOs.DTOs.GestorDocumental.Editor;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Models.Models.GestorDocumental.Editor;
using MiApp.Services.Service.GestorDocumental.Editor;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using System.Threading;
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
        public async Task<ActionResult<AppResponses<RaEditorDocument?>>> FullSave(
            [FromBody] FullSaveEditorDocumentRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var requestId = HttpContext?.TraceIdentifier;
            var aliasClaim = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!aliasClaim.Success || string.IsNullOrWhiteSpace(aliasClaim.ClaimValue))
            {
                return Unauthorized(EditorDocumentErrorResponseFactory.Build<RaEditorDocument?>(
                    EditorDocumentErrorCatalog.AliasInvalid,
                    requestId));
            }

            var actorClaim = _claimValidationService.ValidateClaim<string>("usuarioid");
            if (!actorClaim.Success || string.IsNullOrWhiteSpace(actorClaim.ClaimValue))
            {
                return Unauthorized(EditorDocumentErrorResponseFactory.Build<RaEditorDocument?>(
                    EditorDocumentErrorCatalog.ActorInvalid,
                    requestId));
            }

            var result = await _fullSaveService.FullSaveAsync(
                request,
                aliasClaim.ClaimValue,
                actorClaim.ClaimValue,
                requestId,
                cancellationToken);

            if (result == null || !result.success)
            {
                return StatusCode(EditorDocumentErrorResponseFactory.ResolveHttpStatus(result), result);
            }

            return Ok(result);
        }
    }
}
