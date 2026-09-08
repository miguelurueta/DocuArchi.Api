using MiApp.DTOs.DTOs.GestorDocumental.ContextoDocumental;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.ContextoDocumental;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.GestorDocumental.ContextoDocumental;

[Authorize]
[ApiController]
[Route("api/gestor-documental/radicados/{radicado}/contexto-documental")]
public sealed class ContextoDocumentalRadicadoController(
    IClaimValidationService claimValidationService,
    IContextoDocumentalRadicadoService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(AppResponses<ContextoDocumentalRadicadoDto?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppResponses<ContextoDocumentalRadicadoDto?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AppResponses<ContextoDocumentalRadicadoDto?>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(AppResponses<ContextoDocumentalRadicadoDto?>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(AppResponses<ContextoDocumentalRadicadoDto?>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AppResponses<ContextoDocumentalRadicadoDto?>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(AppResponses<ContextoDocumentalRadicadoDto?>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AppResponses<ContextoDocumentalRadicadoDto?>>> Get(
        [FromRoute] string radicado,
        [FromQuery] ContextoDocumentalRadicadoQueryDto query,
        CancellationToken cancellationToken)
    {
        var requestId = HttpContext.TraceIdentifier;
        var alias = claimValidationService.ValidateClaim<string>("defaulalias");
        var user = claimValidationService.ValidateClaim<string>("usuarioid");
        if (!alias.Success || string.IsNullOrWhiteSpace(alias.ClaimValue) ||
            !user.Success || !int.TryParse(user.ClaimValue, out var usuarioId) || usuarioId <= 0)
            return MapError(ContextoDocumentalErrorCatalog.InvalidRequest, requestId);

        string? workflowAlias = null;
        if (string.Equals(query?.ResolutionSource?.Trim(), "WORKFLOW", StringComparison.OrdinalIgnoreCase))
        {
            var workflow = claimValidationService.ValidateClaim<string>("defaulaliaswf");
            if (workflow.Success && !string.IsNullOrWhiteSpace(workflow.ClaimValue))
                workflowAlias = workflow.ClaimValue.Trim();
        }

        var result = await service.ResolveAsync(new ContextoDocumentalRequest(
            radicado, query?.SourceModule, query?.ResolutionSource, query?.Tipo,
            query?.IdRutaWorkflow, query?.IdTareaWorkflow, usuarioId, alias.ClaimValue.Trim(),
            workflowAlias, requestId), cancellationToken);

        if (!result.Success) return MapError(result.Code, requestId);
        return Ok(new AppResponses<ContextoDocumentalRadicadoDto?>
        {
            success = true,
            message = "OK",
            data = result.Data,
            meta = new AppMeta
            {
                Status = "ok",
                RequestId = requestId,
                ResponseVersion = "1.0",
                Total = 1
            },
            errors = []
        });
    }

    private ActionResult<AppResponses<ContextoDocumentalRadicadoDto?>> MapError(string code, string requestId)
    {
        var response = ContextoDocumentalErrorResponseFactory.Build(code, requestId);
        return StatusCode(ContextoDocumentalErrorResponseFactory.ResolveHttpStatus(code), response);
    }
}
