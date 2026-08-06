using MiApp.Repository.Repositorio.Administracion.Seguridad.Rbac;
using MiApp.Services.Service.Administracion.Seguridad.Rbac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DocuArchi.Api.Infrastructure.DependencyInjection;

public static class AdministracionServiceCollectionExtensions
{
    public static IServiceCollection AddAdministracion(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IRbacPermissionRepository, RbacPermissionRepository>();
        services.AddScoped<IRbacPermissionService, RbacPermissionService>();
        return services;
    }
}
