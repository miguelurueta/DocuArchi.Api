using System.Diagnostics;
using System.Text.RegularExpressions;
using DocuArchi.Api.Infrastructure.Observability;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.Utilidades;

namespace DocuArchi.Api.Infrastructure.Filters;

/// <summary>Correlaciona respuestas del selector de plantillas y registra contexto seguro en sus rutas existentes.</summary>
public sealed class FiltroDiagnosticoSeleccionPlantilla : IAsyncActionFilter
{
    private static readonly HashSet<string> RutasDeConsulta = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/PlantillaRadicado/plantilla-default",
        "/api/PlantillaRadicado/listaPlantilla",
        "/api/PlantillaRadicado/solicitaAutoCompleteTokenRadicado",
        "/api/radicacion/registrar-entrante",
        "/api/tramite/tramites/solicitaFechaLimiteRespuesta",
        "/api/radicacion/pendientes/estado-activo",
        "/api/radicacion/pendientes/contador",
        "/api/tramite/tramites/apListaRadicadosPendientes"
    };
    private readonly IRequestCorrelationContext _correlacion;
    private readonly ILogger<FiltroDiagnosticoSeleccionPlantilla> _logger;

    /// <summary>Recibe correlación existente y logging estructurado sin acceso a configuración de bases ni documentos.</summary>
    /// <param name="correlacion">Contexto seguro de correlación de la solicitud HTTP actual.</param>
    /// <param name="logger">Registro estructurado que recibe exclusivamente campos controlados.</param>
    public FiltroDiagnosticoSeleccionPlantilla(
        IRequestCorrelationContext correlacion, ILogger<FiltroDiagnosticoSeleccionPlantilla> logger)
    {
        _correlacion = correlacion;
        _logger = logger;
    }

    /// <summary>Conserva el resultado HTTP y correlaciona únicamente fallos del selector en las rutas del componente.</summary>
    /// <param name="context">Contexto MVC que identifica ruta y usuario autenticado.</param>
    /// <param name="next">Continuación del framework, invocada exactamente una vez.</param>
    /// <returns>Tarea completada tras la acción y el registro seguro, sin convertir excepciones ajenas en errores del selector.</returns>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var ruta = context.HttpContext.Request.Path.Value?.TrimEnd('/') ?? string.Empty;
        if (!RutasDeConsulta.Contains(ruta)
            && !Regex.IsMatch(ruta, "^/api/radicacion/pendientes/-?[0-9]+/(enviar-pendiente|tomar)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            await next();
            return;
        }
        var requestId = _correlacion.RequestId;
        var duracion = Stopwatch.StartNew();
        var resultado = await next();
        if (resultado.Result is not ObjectResult { Value: { } respuesta }) return;
        var tipo = respuesta.GetType();
        if (!tipo.IsGenericType || tipo.GetGenericTypeDefinition() != typeof(AppResponses<>)) return;
        if (tipo.GetProperty(nameof(AppResponses<object>.meta))?.GetValue(respuesta) is not AppMeta { Metadata: { } datos } meta) return;
        var operacion = datos.GetValueOrDefault("Operation") as string;
        if (operacion is not ("read-da-template" or "revalidate-da-template" or "validate-da-procedure-template" or "read-da-procedure")) return;

        meta.RequestId = requestId;
        var etapa = operacion.StartsWith("read-", StringComparison.Ordinal) ? "lookup" : "validation";
        var errores = tipo.GetProperty(nameof(AppResponses<object>.errors))?.GetValue(respuesta) as object[];
        var candidatoCodigo = errores?.OfType<AppError>().FirstOrDefault()?.Code;
        var codigo = candidatoCodigo is not null && Regex.IsMatch(candidatoCodigo, "^[A-Za-z0-9_.-]{1,100}$", RegexOptions.CultureInvariant)
            ? candidatoCodigo : "PLANTILLA_RADICACION_DEFAULT_NOT_FOUND";
        var estado = meta.Status is not null && Regex.IsMatch(meta.Status, "^[A-Za-z0-9_.-]{1,100}$", RegexOptions.CultureInvariant)
            ? meta.Status : "ERROR";
        int? usuarioId = int.TryParse(context.HttpContext.User.FindFirst("usuarioid")?.Value, out var usuario) ? usuario : null;
        try
        {
            _logger.LogWarning(
                "stage={Stage} operation={Operation} errorCode={ErrorCode} metaStatus={MetaStatus} durationMs={DurationMs} requestId={RequestId} usuarioId={UsuarioId}",
                etapa, operacion, codigo, estado, duracion.ElapsedMilliseconds, requestId, usuarioId);
        }
        catch (Exception causaSecundaria)
        {
            // Se conserva solo en el contexto interno; nunca se serializa ni se registra como texto crudo.
            context.HttpContext.Items[typeof(FiltroDiagnosticoSeleccionPlantilla)] = causaSecundaria;
            datos["DiagnosticLoggingFailure"] = new Dictionary<string, object?>
            {
                ["Operation"] = "log-template-selection", ["TechnicalCode"] = "PLANTILLA_DIAGNOSTICO_LOG_FALLO",
                ["TechnicalDetail"] = "No fue posible registrar el diagnóstico; el rechazo principal se conserva."
            };
        }
    }
}
