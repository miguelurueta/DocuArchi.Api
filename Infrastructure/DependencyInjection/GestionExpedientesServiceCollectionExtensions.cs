using MiApp.Repository.Repositorio.GestorDocumental.Expedientes;
using MiApp.Services.Service.GestorDocumental.Expedientes;

namespace Microsoft.Extensions.DependencyInjection;

public static class GestionExpedientesServiceCollectionExtensions
{
    public static IServiceCollection AddGestionExpedientes(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IExpedienteRepository, ExpedienteRepository>();
        services.AddScoped<IExpedienteService, ExpedienteService>();
        return services;
    }
}
