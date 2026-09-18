using MiApp.Repository.Repositorio.GestorDocumental.Expedientes;
using MiApp.Services.Service.GestorDocumental.Expedientes;
using MiApp.Repository.DataAccess;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class GestionExpedientesServiceCollectionExtensions
{
    public static IServiceCollection AddGestionExpedientes(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddScoped<IDapperCrudTransactionalEngine>(provider =>
            provider.GetRequiredService<IDapperCrudEngine>() as IDapperCrudTransactionalEngine
            ?? throw new InvalidOperationException("The configured DapperCrudEngine does not support transactions."));
        services.AddScoped<IExpedienteRepository, ExpedienteRepository>();
        services.AddScoped<IExpedienteService, ExpedienteService>();
        services.AddScoped<IExpedienteTypeCatalogRepository, ExpedienteTypeCatalogRepository>();
        services.AddScoped<IArchivoCatalogRepository, ArchivoCatalogRepository>();
        services.AddScoped<IOrganizationalStructureCatalogRepository, OrganizationalStructureCatalogRepository>();
        services.AddScoped<IResponsableGestionRepository, ResponsableGestionRepository>();
        services.AddScoped<IExpedienteCatalogService, ExpedienteCatalogService>();
        services.AddScoped<IExpedienteCreateModelService, ExpedienteCreateModelService>();
        services.AddScoped<IExpedienteWriteRepository, ExpedienteWriteRepository>();
        services.AddScoped<IExpedienteIndiceXmlInitializer, ExpedienteIndiceXmlInitializer>();
        services.AddScoped<ICrearExpedienteUseCase, CrearExpedienteUseCase>();
        return services;
    }
}
