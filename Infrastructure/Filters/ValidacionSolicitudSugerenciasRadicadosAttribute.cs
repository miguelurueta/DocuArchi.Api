using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MiApp.Services.Service.Radicacion.Radicados.Autocomplete.Errors;
namespace DocuArchi.Api.Infrastructure.Filters;

/// <summary>Uniforma errores de binding sólo en el endpoint nuevo, antes del filtro automático de ApiController.</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ValidacionSolicitudSugerenciasRadicadosAttribute : ActionFilterAttribute
{
    /// <summary>Fija la precedencia requerida para el binding de esta acción.</summary>
    public ValidacionSolicitudSugerenciasRadicadosAttribute() => Order = -3000;

    /// <summary>Devuelve error funcional sin copiar mensajes crudos de JsonException ni alterar otras acciones.</summary>
    /// <param name="context">Contexto de binding con claves conocidas, sin registrar su contenido.</param>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid) return;
        var camposInvalidos = context.ModelState.Where(x => x.Value?.Errors.Count > 0)
            .Select(x => x.Key.Split('.').Last().TrimStart('$').ToLowerInvariant()).ToHashSet(StringComparer.Ordinal);
        // El error agregado del parámetro requerido no debe ocultar el campo que falló al deserializar.
        var campo = new[] { "idplantilla", "texto", "tipo", "limite" }.FirstOrDefault(camposInvalidos.Contains);
        var codigo = campo switch {
            "idplantilla" => "PLANTILLA_INVALIDA", "texto" => "TEXTO_INVALIDO", "tipo" => "TIPO_INVALIDO", "limite" => "LIMITE_INVALIDO", _ => "SOLICITUD_INVALIDA" };
        var errores = context.HttpContext.RequestServices.GetRequiredService<ErroresSugerenciasRadicados>();
        context.Result = new BadRequestObjectResult(errores.CrearErrorSugerenciasRadicados(codigo,
            context.HttpContext.TraceIdentifier, codigoTecnico: "SOLICITUD_NO_LEGIBLE"));
    }
}
