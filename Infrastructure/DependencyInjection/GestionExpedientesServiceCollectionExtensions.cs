using MiApp.Repository.Repositorio.GestorDocumental.Expedientes;
using MiApp.Services.Service.GestorDocumental.Expedientes;

namespace Microsoft.Extensions.DependencyInjection;

public static class GestionExpedientesServiceCollectionExtensions
{
    public static IServiceCollection AddGestionExpedientes(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IExpedienteRepository, ExpedienteRepository>();
        services.AddScoped<IExpedienteService, ExpedienteService>();
        services.AddScoped<IExpedienteTypeCatalogRepository, ExpedienteTypeCatalogRepository>();
        services.AddScoped<IArchivoCatalogRepository, ArchivoCatalogRepository>();
        services.AddScoped<IOrganizationalStructureCatalogRepository, OrganizationalStructureCatalogRepository>();
        services.AddScoped<IResponsableGestionRepository, ResponsableGestionRepository>();
        services.AddScoped<IExpedienteCatalogService, ExpedienteCatalogService>();
        services.AddScoped<IExpedienteCreateModelService, ExpedienteCreateModelService>();
        return services;
    }
}
