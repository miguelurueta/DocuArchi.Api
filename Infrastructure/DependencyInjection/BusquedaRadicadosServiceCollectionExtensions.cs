using MiApp.Repository.Repositorio.Radicador.Radicados.Autocomplete;
using MiApp.Services.Service.Radicacion.Radicados.Autocomplete;
using MiApp.Services.Service.Radicacion.Radicados.Autocomplete.Errors;
namespace DocuArchi.Api.Infrastructure.DependencyInjection;

/// <summary>Composición modular del autocomplete nuevo, reutilizando la resolución documental ya registrada.</summary>
public static class BusquedaRadicadosServiceCollectionExtensions
{
    /// <summary>Registra contratos e implementaciones propios de búsqueda documental.</summary>
    /// <param name="services">Contenedor de la API, con resolución compartida de Radicación.</param>
    /// <param name="configuration">Configuración recibida por convención del módulo, sin cambiar JSON global.</param>
    /// <returns>El mismo contenedor con las dependencias específicas registradas.</returns>
    public static IServiceCollection AddBusquedaRadicados(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IBusquedaRadicadosRepository, BusquedaRadicadosRepository>();
        services.AddScoped<IBusquedaRadicadosService, BusquedaRadicadosService>();
        services.AddScoped<ErroresSugerenciasRadicados>();
        return services;
    }
}
