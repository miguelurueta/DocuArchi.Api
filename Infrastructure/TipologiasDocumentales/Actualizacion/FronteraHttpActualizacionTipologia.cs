using System.Text.Json;
using System.Text.RegularExpressions;
using MiApp.DTOs.DTOs.GestorDocumental.TipologiasDocumentales.Actualizacion;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.TipologiasDocumentales.Actualizacion;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Net.Http.Headers;

namespace DocuArchi.Api.Infrastructure.TipologiasDocumentales.Actualizacion;

/// <summary>Normaliza únicamente transporte/binding/excepciones del Controller nuevo, preservando las opciones MVC globales.</summary>
public sealed class FronteraHttpActualizacionTipologia : IAsyncResourceFilter, IAsyncActionFilter, IAsyncExceptionFilter
{
    internal static readonly JsonSerializerOptions OpcionesJson = new() { PropertyNamingPolicy = null };
    private readonly IErrorActualizacionTipologiaFactory _errores;

    /// <summary>Recibe el catálogo seguro compartido con el servicio y la autenticación del endpoint.</summary>
    /// <param name="errores">Factoría que conserva código/mensaje/correlación sin publicar el texto del parser.</param>
    public FronteraHttpActualizacionTipologia(IErrorActualizacionTipologiaFactory errores) { _errores = errores; }

    /// <summary>Valida cuerpo negociable y Content-Type antes del binding y de cualquier efecto de negocio.</summary>
    /// <param name="context">Contexto MVC exclusivo del Controller marcado con este filtro.</param>
    /// <param name="next">Resto del recorrido de recursos/binding cuando el transporte es válido.</param>
    /// <returns>Tarea con respuesta propia de 400/406/415 o continuación del pipeline.</returns>
    /// <remarks>Nombre impuesto por IAsyncResourceFilter.OnResourceExecutionAsync; excepción LEG-07.</remarks>
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        ObtenerCorrelacionSegura(context.HttpContext);
        if (!AdmiteRespuestaJson(context.HttpContext))
        { context.Result = CrearError(context.HttpContext, "TIPDOC-UPD-RESPONSE-TYPE-NOT-ACCEPTABLE"); return; }
        if (context.HttpContext.Features.Get<IHttpRequestBodyDetectionFeature>()?.CanHaveBody == false || context.HttpContext.Request.ContentLength == 0)
        { context.Result = CrearError(context.HttpContext, "TIPDOC-UPD-REQUEST-INVALID"); return; }
        if (!MediaTypeHeaderValue.TryParse(context.HttpContext.Request.ContentType, out var contenido) ||
            !string.Equals(contenido.MediaType.Value, "application/json", StringComparison.OrdinalIgnoreCase))
        { context.Result = CrearError(context.HttpContext, "TIPDOC-UPD-CONTENT-TYPE-NOT-SUPPORTED"); return; }
        await next().ConfigureAwait(false);
    }

    /// <summary>Sustituye errores de ModelState solo en este endpoint y conserva la primera excepción disponible para el diagnóstico seguro.</summary>
    /// <param name="context">Errores de binding y argumentos, cuyo texto crudo no se publica.</param>
    /// <param name="next">Acción cuando la petición específica es válida.</param>
    /// <returns>Tarea con error controlado o ejecución del Controller.</returns>
    /// <remarks>Nombre impuesto por IAsyncActionFilter.OnActionExecutionAsync; excepción LEG-07.</remarks>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid || !context.ActionArguments.Values.Any(x => x is ActualizarTipologiaWorkflowRadicacionRequestDto))
        {
            var errores = context.ModelState.Values.SelectMany(x => x.Errors).ToArray();
            var contenido = errores.Any(x => x.Exception is UnsupportedContentTypeException);
            var causa = errores.Select(x => x.Exception).FirstOrDefault(x => x is not null);
            context.Result = CrearError(context.HttpContext, contenido ? "TIPDOC-UPD-CONTENT-TYPE-NOT-SUPPORTED" : "TIPDOC-UPD-REQUEST-INVALID", causa);
            return;
        }
        await next().ConfigureAwait(false);
    }

    /// <summary>Traduce excepciones no resueltas por la aplicación sin exponer detalles de Development ni datos del request.</summary>
    /// <param name="context">Excepción original de la ejecución MVC de este Controller.</param>
    /// <returns>Tarea completada con error seguro, salvo errores de transporte tras comenzar la respuesta.</returns>
    /// <remarks>Nombre impuesto por IAsyncExceptionFilter.OnExceptionAsync; excepción LEG-07.</remarks>
    public Task OnExceptionAsync(ExceptionContext context)
    {
        if (!context.HttpContext.Response.HasStarted)
        {
            context.Result = CrearError(context.HttpContext, "TIPDOC-UPD-UNEXPECTED", context.Exception);
            context.ExceptionHandled = true;
        }
        return Task.CompletedTask;
    }

    /// <summary>Conserva una correlación del servidor válida durante todo el recorrido, sin admitir identificadores libres del body.</summary>
    /// <param name="contexto">Contexto HTTP actual.</param>
    /// <returns>Identificador compartido por respuesta y logs.</returns>
    internal static string ObtenerCorrelacionSegura(HttpContext contexto)
    {
        if (!Regex.IsMatch(contexto.TraceIdentifier ?? string.Empty, @"\A[A-Za-z0-9._:\-]{1,100}\z", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100)))
            contexto.TraceIdentifier = Guid.NewGuid().ToString("N");
        return contexto.TraceIdentifier;
    }

    /// <summary>Usa serialización local del contrato, preservando PascalCase y null explícito sin opciones globales nuevas.</summary>
    /// <param name="estado">HTTP acreditado por la operación.</param>
    /// <param name="respuesta">Respuesta segura del contrato nuevo.</param>
    /// <returns>Resultado JSON de esta ruta.</returns>
    internal static JsonResult CrearResultadoJson(int estado, AppResponses<ActualizarTipologiaDocumentalResponseDto> respuesta) =>
        new(respuesta, OpcionesJson) { StatusCode = estado, ContentType = "application/json" };

    /// <summary>Proyecta un error de frontera con correlación y causa privada cuando existe.</summary>
    /// <param name="contexto">Contexto de la petición.</param>
    /// <param name="codigo">Código controlado de binding/transporte/excepción.</param>
    /// <param name="causa">Causa original sin publicar su texto.</param>
    /// <returns>Respuesta JSON contractual.</returns>
    private JsonResult CrearError(HttpContext contexto, string codigo, Exception? causa = null)
    {
        var error = _errores.CrearRespuestaError(codigo, ObtenerCorrelacionSegura(contexto), "binding", "LeerSolicitudActualizacionTipologia", 0, causa: causa);
        return CrearResultadoJson(error.EstadoHttp, error.Respuesta);
    }

    /// <summary>Respeta calidad cero y prioridad del rango más específico al negociar el único formato publicado.</summary>
    /// <param name="contexto">Headers de la petición, sin incorporarlos a logs ni mensajes.</param>
    /// <returns>True si application/json es aceptable; false ante formato incompatible o header inválido.</returns>
    private static bool AdmiteRespuestaJson(HttpContext contexto)
    {
        try
        {
            var aceptados = contexto.Request.GetTypedHeaders().Accept;
            if (aceptados is null || aceptados.Count == 0) return true;
            var candidatos = aceptados.Select(x => new
            {
                Prioridad = x.MediaType.Value?.ToLowerInvariant() switch { "application/json" => 2, "application/*" => 1, "*/*" => 0, _ => -1 },
                Calidad = x.Quality ?? 1
            }).Where(x => x.Prioridad >= 0).ToArray();
            if (candidatos.Length == 0) return false;
            var prioridad = candidatos.Max(x => x.Prioridad);
            return candidatos.Where(x => x.Prioridad == prioridad).Any(x => x.Calidad > 0);
        }
        catch (FormatException) { return false; }
    }
}
