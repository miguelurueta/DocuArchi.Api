using MiApp.Repository.Repositorio.GestorDocumental.Expedientes;
using MiApp.Services.Service.GestorDocumental.Expedientes;
using MiApp.Services.Service.GestorDocumental.InstrumentosArchivisticos;
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
        services.AddScoped<IExpedienteCatalogService, ExpedienteCatalogService>();
        services.AddScoped<IExpedienteCycleContextService, ExpedienteCycleContextService>();
        services.AddScoped<IExpedienteCreateModelService, ExpedienteCreateModelService>();
        services.AddScoped<IInitialExpedienteContextResolver, InitialExpedienteContextResolver>();
        services.AddScoped<IExpedienteWriteRepository, ExpedienteWriteRepository>();
        services.AddScoped<IExpedienteIndiceXmlInitializer, ExpedienteIndiceXmlInitializer>();
        services.AddScoped<ICrearExpedienteUseCase, CrearExpedienteUseCase>();
        services.AddScoped<IExpedienteEditModelService, ExpedienteEditModelService>();
        services.AddScoped<IEditarExpedienteUseCase, EditarExpedienteUseCase>();
        services.AddScoped<IExpedienteUpdateValidator, ExpedienteUpdateValidator>();
        services.AddScoped<IExpedientePermissionPolicy, ExpedientePermissionPolicy>();
        services.AddScoped<IExpedienteEstadoPolicy, ExpedienteEstadoPolicy>();
        services.AddScoped<IExpedienteOwnershipPolicy, ExpedienteOwnershipPolicy>();
        services.AddScoped<IExpedienteAutoRegistroPolicy, ExpedienteAutoRegistroPolicy>();
        services.AddScoped<IExpedienteRetencionService, ExpedienteRetencionService>();
        return services;
    }
}
