using DocuArchi.Api.Infrastructure.Features;
using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.Radicacion.Terminar;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.Radicacion.Terminar;
using MiApp.Services.Service.Radicacion.Terminar.Errors;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.Radicacion.Terminar;

/// <summary>
/// Expone una politica declarativa de terminacion. Esta consulta no muta Workflow ni Radicacion.
/// </summary>
[Route("api/radicacion/estados")]
[ApiController]
[Authorize]
public sealed class RadicationCompletionPolicyController : ControllerBase
{
    private const string FeatureName = "RADICACION_TERMINATION_POLICY";

    private readonly IFeatureToggleService _featureToggleService;
    private readonly IClaimValidationService _claimValidationService;
    private readonly IRadicationCompletionPolicyService _policyService;

    public RadicationCompletionPolicyController(
        IFeatureToggleService featureToggleService,
        IClaimValidationService claimValidationService,
        IRadicationCompletionPolicyService policyService)
    {
        _featureToggleService = featureToggleService;
        _claimValidationService = claimValidationService;
        _policyService = policyService;
    }

    /// <summary>
    /// Consulta la accion de terminacion permitida para un estado de radicacion, sin ejecutarla.
    /// </summary>
    [HttpGet("{idEstadoRadicado:long}/politica-terminacion")]
    [ProducesResponseType(typeof(AppResponses<RadicationCompletionPolicyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppResponses<RadicationCompletionPolicyDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AppResponses<RadicationCompletionPolicyDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(AppResponses<RadicationCompletionPolicyDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(AppResponses<RadicationCompletionPolicyDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AppResponses<RadicationCompletionPolicyDto>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(AppResponses<RadicationCompletionPolicyDto>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(AppResponses<RadicationCompletionPolicyDto>), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<AppResponses<RadicationCompletionPolicyDto>>> GetPolicy(
        long idEstadoRadicado,
        CancellationToken cancellationToken)
    {
        var requestId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");

        try
        {
            if (!await _featureToggleService.IsEnabledAsync(FeatureName))
            {
                return FromError(RadicationCompletionPolicyErrorCatalog.Disabled, requestId);
            }

            var validationAlias = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!validationAlias.Success || string.IsNullOrWhiteSpace(validationAlias.ClaimValue))
            {
                return FromError(RadicationCompletionPolicyErrorCatalog.AccessDenied, requestId, "defaulalias");
            }

            var validationUsuario = _claimValidationService.ValidateClaim<string>("usuarioid");
            if (!validationUsuario.Success ||
                !int.TryParse(validationUsuario.ClaimValue, out var idUsuarioGestion) ||
                idUsuarioGestion <= 0)
            {
                return FromError(RadicationCompletionPolicyErrorCatalog.AccessDenied, requestId, "usuarioid");
            }

            var validationAliasWorkflow = _claimValidationService.ValidateClaim<string>("defaulaliaswf");
            var defaultWorkflowAlias = validationAliasWorkflow.Success
                ? validationAliasWorkflow.ClaimValue
                : null;

            var result = await _policyService.GetAsync(
                idEstadoRadicado,
                idUsuarioGestion,
                validationAlias.ClaimValue,
                defaultWorkflowAlias,
                requestId,
                cancellationToken);

            return result.success ? Ok(result) : FromResultError(result, requestId);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return FromError(RadicationCompletionPolicyErrorCatalog.Unexpected, requestId);
        }
    }

    private ActionResult<AppResponses<RadicationCompletionPolicyDto>> FromResultError(
        AppResponses<RadicationCompletionPolicyDto> result,
        string requestId)
    {
        var errorCode = result.errors?.OfType<AppError>().FirstOrDefault()?.Code;
        if (string.IsNullOrWhiteSpace(errorCode))
        {
            return FromError(RadicationCompletionPolicyErrorCatalog.Unexpected, requestId);
        }

        var descriptor = RadicationCompletionPolicyErrorCatalog.Get(errorCode);
        return StatusCode(descriptor.HttpStatus, result);
    }

    private ActionResult<AppResponses<RadicationCompletionPolicyDto>> FromError(
        string errorCode,
        string requestId,
        string? field = null)
    {
        var descriptor = RadicationCompletionPolicyErrorCatalog.Get(errorCode);
        var response = RadicationCompletionPolicyErrorResponseFactory.Create<RadicationCompletionPolicyDto>(
            errorCode,
            requestId,
            field);
        return StatusCode(descriptor.HttpStatus, response);
    }
}
