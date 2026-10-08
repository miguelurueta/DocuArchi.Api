using MiApp.DTOs.DTOs.GestorDocumental.Editor;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.Editor;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.GestorDocumental.Editor;

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
        [FromQuery] string? prefer = null,
        CancellationToken cancellationToken = default)
    {
        var requestId = HttpContext?.TraceIdentifier;
        var aliasClaim = _claimValidationService.ValidateClaim<string>("defaulalias");
        if (!aliasClaim.Success || string.IsNullOrWhiteSpace(aliasClaim.ClaimValue))
        {
            return Unauthorized(EditorDocumentErrorResponseFactory.Build<EditorResolveDocumentResponseDto?>(
                EditorDocumentErrorCatalog.AliasInvalid,
                requestId));
        }

        if (string.IsNullOrWhiteSpace(contextCode))
        {
            return BadRequest(EditorDocumentErrorResponseFactory.Build<EditorResolveDocumentResponseDto?>(
                EditorDocumentErrorCatalog.RequestInvalid,
                requestId,
                "contextCode"));
        }

        if (entityId <= 0)
        {
            return BadRequest(EditorDocumentErrorResponseFactory.Build<EditorResolveDocumentResponseDto?>(
                EditorDocumentErrorCatalog.RequestInvalid,
                requestId,
                "entityId"));
        }

        if (!string.IsNullOrWhiteSpace(prefer))
        {
            var normalizedPreference = prefer.Trim().ToLowerInvariant();
            if (normalizedPreference != "existing" && normalizedPreference != "initial")
            {
                return BadRequest(EditorDocumentErrorResponseFactory.Build<EditorResolveDocumentResponseDto?>(
                    EditorDocumentErrorCatalog.RequestInvalid,
                    requestId,
                    "prefer"));
            }
        }

        var result = await _service.ResolveAsync(
            idTareaWf,
            contextCode,
            entityId,
            templateDefinitionId,
            templateCode,
            prefer,
            aliasClaim.ClaimValue,
            requestId,
            cancellationToken);

        if (result == null)
        {
            return StatusCode(500, EditorDocumentErrorResponseFactory.Build<EditorResolveDocumentResponseDto?>(
                EditorDocumentErrorCatalog.Unexpected,
                requestId));
        }

        if (!result.success)
        {
            return StatusCode(EditorDocumentErrorResponseFactory.ResolveHttpStatus(result), result);
        }

        return Ok(result);
    }
}
