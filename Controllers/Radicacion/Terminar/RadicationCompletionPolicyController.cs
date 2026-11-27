using System.Diagnostics;
using DocuArchi.Api.Infrastructure.Features;
using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.Radicacion.Terminar;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Models.Models.Radicacion.Terminar;
using MiApp.Models.Models.Workflow.Terminar;
using MiApp.Services.Service.Radicacion.Terminar;
using MiApp.Services.Service.Radicacion.Terminar.Errors;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using MiApp.Services.Service.Workflow.Terminar.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace DocuArchi.Api.Controllers.Radicacion.Terminar;

/// <summary>Expone la política declarativa de terminación y su diagnóstico seguro sin mutar Workflow ni Radicación.</summary>
[Route("api/radicacion/estados")]
[ApiController]
[Authorize]
public sealed class RadicationCompletionPolicyController : ControllerBase
{
    private const string FeatureName = "RADICACION_TERMINATION_POLICY";
    private readonly IFeatureToggleService _featureToggleService;
    private readonly IClaimValidationService _claimValidationService;
    private readonly IRadicationCompletionPolicyService _policyService;
    private readonly ILogger<RadicationCompletionPolicyController> _logger;

    /// <summary>Conserva la construcción histórica para consumidores directos sin proveedor de logs.</summary>
    /// <param name="featureToggleService">Consulta la bandera de la política.</param>
    /// <param name="claimValidationService">Valida los claims existentes.</param>
    /// <param name="policyService">Consulta la política sin escrituras.</param>
    public RadicationCompletionPolicyController(
        IFeatureToggleService featureToggleService,
        IClaimValidationService claimValidationService,
        IRadicationCompletionPolicyService policyService)
        : this(featureToggleService, claimValidationService, policyService,
            NullLogger<RadicationCompletionPolicyController>.Instance) { }

    /// <summary>Compone la frontera con el logger estructurado del host sin alterar permisos ni reglas.</summary>
    /// <param name="featureToggleService">Consulta la bandera de la política.</param>
    /// <param name="claimValidationService">Valida los claims autenticados requeridos.</param>
    /// <param name="policyService">Caso de uso de consulta de política.</param>
    /// <param name="logger">Registro estructurado de diagnóstico sin excepciones ni datos de negocio.</param>
    [Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructor]
    public RadicationCompletionPolicyController(
        IFeatureToggleService featureToggleService,
        IClaimValidationService claimValidationService,
        IRadicationCompletionPolicyService policyService,
        ILogger<RadicationCompletionPolicyController> logger)
    {
        _featureToggleService = featureToggleService;
        _claimValidationService = claimValidationService;
        _policyService = policyService;
        _logger = logger;
    }

    /// <summary>Consulta la acción de terminación sin ejecutarla y conserva diagnóstico/correlación de los rechazos.</summary>
    /// <param name="idEstadoRadicado">Identificador del estado solicitado en la ruta existente.</param>
    /// <param name="cancellationToken">Cancelación de la solicitud que se propaga sin convertirla en error público.</param>
    /// <returns>Política completa o respuesta HTTP compatible con diagnóstico seguro incluso en Production.</returns>
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
        long idEstadoRadicado, CancellationToken cancellationToken)
    {
        var requestId = RadicationCompletionPolicyErrorResponseFactory.NormalizarIdentificadorSolicitud(HttpContext?.TraceIdentifier);
        if (HttpContext is not null) HttpContext.TraceIdentifier = requestId;
        var timer = Stopwatch.StartNew();
        int? actor = null;
        var origin = RadicationCompletionPolicyOperation.FeatureFlag;
        try
        {
            if (!await _featureToggleService.IsEnabledAsync(FeatureName))
                return ConstruirRechazoPolitica(RadicationCompletionPolicyErrorCatalog.Disabled,
                    requestId, new(RadicationCompletionPolicyFailure.FeatureDisabled), timer.ElapsedMilliseconds, actor);

            origin = RadicationCompletionPolicyOperation.Claims;
            var validationAlias = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!validationAlias.Success || string.IsNullOrWhiteSpace(validationAlias.ClaimValue))
                return ConstruirRechazoPolitica(RadicationCompletionPolicyErrorCatalog.AccessDenied,
                    requestId, new(RadicationCompletionPolicyFailure.RequiredClaimInvalid), timer.ElapsedMilliseconds, actor, "defaulalias");
            var validationUsuario = _claimValidationService.ValidateClaim<string>("usuarioid");
            if (!validationUsuario.Success || !int.TryParse(validationUsuario.ClaimValue, out var idUsuarioGestion) || idUsuarioGestion <= 0)
                return ConstruirRechazoPolitica(RadicationCompletionPolicyErrorCatalog.AccessDenied,
                    requestId, new(RadicationCompletionPolicyFailure.RequiredClaimInvalid), timer.ElapsedMilliseconds, actor, "usuarioid");
            actor = idUsuarioGestion;
            var validationAliasWorkflow = _claimValidationService.ValidateClaim<string>("defaulaliaswf");
            var defaultWorkflowAlias = validationAliasWorkflow.Success ? validationAliasWorkflow.ClaimValue : null;

            origin = RadicationCompletionPolicyOperation.PolicyResult;
            var result = await _policyService.GetAsync(idEstadoRadicado, idUsuarioGestion,
                validationAlias.ClaimValue, defaultWorkflowAlias, requestId, cancellationToken);
            return ConstruirResultadoHttpPolitica(result, requestId, timer.ElapsedMilliseconds, actor);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception exception)
        {
            return ConstruirRechazoPolitica(RadicationCompletionPolicyErrorCatalog.Unexpected, requestId,
                new(RadicationCompletionPolicyFailure.Unexpected,
                    WorkflowTerminationDiagnostics.FromException(exception), Origin: origin),
                timer.ElapsedMilliseconds, actor);
        }
    }

    /// <summary>Construye un rechazo de frontera mediante la fábrica y lo registra con el mismo contexto HTTP.</summary>
    /// <param name="code">Código público vigente del rechazo.</param>
    /// <param name="requestId">Correlación segura de la solicitud.</param>
    /// <param name="diagnostic">Condición tipada observada en la frontera.</param>
    /// <param name="durationMs">Duración transcurrida del GET.</param>
    /// <param name="actor">Usuario de Gestión cuando ya pudo resolverse.</param>
    /// <param name="field">Campo público asociado al rechazo cuando aplica.</param>
    /// <returns>Respuesta HTTP con status histórico y metadata segura.</returns>
    private ActionResult<AppResponses<RadicationCompletionPolicyDto>> ConstruirRechazoPolitica(
        string code, string requestId, RadicationCompletionPolicyDiagnostic diagnostic,
        long durationMs, int? actor, string? field = null) =>
        ConstruirResultadoHttpPolitica(
            RadicationCompletionPolicyErrorResponseFactory.CrearRespuestaError<RadicationCompletionPolicyDto>(
                code, requestId, field, diagnostic), requestId, durationMs, actor);

    /// <summary>Resuelve el mismo status del catálogo y registra solo campos controlados de diagnóstico y correlación.</summary>
    /// <param name="response">Resultado del servicio o fábrica sin modificar su mensaje funcional.</param>
    /// <param name="requestId">Identificador compartido entre frontera, servicio y logs.</param>
    /// <param name="durationMs">Tiempo transcurrido en milisegundos.</param>
    /// <param name="actor">Identidad de Gestión ya resuelta y pertinente.</param>
    /// <returns>Respuesta exitosa o rechazo serializable con el HTTP vigente.</returns>
    private ActionResult<AppResponses<RadicationCompletionPolicyDto>> ConstruirResultadoHttpPolitica(
        AppResponses<RadicationCompletionPolicyDto> response, string requestId, long durationMs, int? actor)
    {
        var code = response.errors?.OfType<AppError>().FirstOrDefault()?.Code;
        if (!response.success && string.IsNullOrWhiteSpace(code))
        {
            response = RadicationCompletionPolicyErrorResponseFactory.CrearRespuestaError<RadicationCompletionPolicyDto>(
                RadicationCompletionPolicyErrorCatalog.Unexpected, requestId,
                diagnostico: new(RadicationCompletionPolicyFailure.PolicyResultIncomplete));
            code = RadicationCompletionPolicyErrorCatalog.Unexpected;
        }
        response.meta ??= new AppMeta();
        response.meta.RequestId = requestId;
        if (!response.success && response.meta.Metadata is null)
            response.meta.Metadata = RadicationCompletionPolicyErrorResponseFactory.Create<RadicationCompletionPolicyDto>(
                code!, requestId).meta!.Metadata;
        var metadata = response.meta.Metadata;
        _logger.Log(response.success ? LogLevel.Information : LogLevel.Warning,
            "Radication completion policy. requestId={RequestId} stage={Stage} operation={Operation} errorCode={ErrorCode} technicalCode={TechnicalCode} metaStatus={MetaStatus} durationMs={DurationMs} usuarioId={UsuarioId}",
            requestId, metadata?.GetValueOrDefault("Stage") ?? "policy",
            metadata?.GetValueOrDefault("Operation") ?? "query-completion-policy",
            code, metadata?.GetValueOrDefault("TechnicalCode"), response.meta.Status, durationMs, actor);
        return response.success ? Ok(response)
            : StatusCode(RadicationCompletionPolicyErrorCatalog.Get(code!).HttpStatus, response);
    }
}
