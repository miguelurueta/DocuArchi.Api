using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging.Abstractions;

namespace DocuArchi.Api.Infrastructure.TipologiasDocumentales.Actualizacion;

/// <summary>Selecciona el enlace de body propio solo para el parámetro del comando de actualización.</summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class SolicitudActualizacionTipologiaDesdeJsonAttribute : FromBodyAttribute, IBinderTypeProviderMetadata
{
    /// <summary>Enlazador que conserva la excepción JSON para la frontera segura de este comando.</summary>
    public Type BinderType => typeof(EnlazadorSolicitudActualizacionTipologia);
}

/// <summary>Reutiliza el enlace y formateador MVC de JSON con conservación de causa limitada a esta petición.</summary>
public sealed class EnlazadorSolicitudActualizacionTipologia : IModelBinder
{
    private readonly BodyModelBinder _enlazador;

    /// <summary>Prepara el enlace del comando sin cambiar formateadores ni opciones compartidas de la API.</summary>
    /// <param name="lectores">Fábrica MVC de lectores del body, conservando las codificaciones soportadas por el framework.</param>
    public EnlazadorSolicitudActualizacionTipologia(IHttpRequestStreamReaderFactory lectores)
    {
        var opciones = new JsonOptions { AllowInputFormatterExceptionMessages = false };
        opciones.JsonSerializerOptions.PropertyNamingPolicy = null;
        var formato = new SystemTextJsonInputFormatter(opciones, NullLogger<SystemTextJsonInputFormatter>.Instance);
        _enlazador = new BodyModelBinder([formato], lectores, NullLoggerFactory.Instance);
    }

    /// <summary>Enlaza el body mediante MVC y conserva la excepción disponible para el filtro propio, sin publicar su texto.</summary>
    /// <param name="bindingContext">Contexto del único parámetro marcado; recibe resultado y ModelState del enlace.</param>
    /// <returns>Tarea del enlace; el filtro propio decide la respuesta contractual ante ModelState inválido.</returns>
    /// <remarks>Nombre exigido por Microsoft.AspNetCore.Mvc.ModelBinding.IModelBinder.BindModelAsync; excepción LEG-07.</remarks>
    public Task BindModelAsync(ModelBindingContext bindingContext) => _enlazador.BindModelAsync(bindingContext);
}
