using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DocuArchi.Api.Infrastructure.DependencyInjection;

public static class EmailServiceCollectionExtensions
{
    public static IServiceCollection AddEmailDispatchEngine(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddEmailDispatchRepositories();
        services.TryAddScoped<MiApp.Services.Service.Email.IServicioCorreo, MiApp.Services.Service.Email.ServicioCorreo>();
        services.AddOptions<MiApp.Services.Service.Email.EmailDispatchOptions>()
            .Bind(configuration.GetSection("EmailDispatch"));
        if (configuration.GetValue<bool>("EmailDispatch:EnableDispatchService"))
        {
            services.TryAddScoped<
                MiApp.Services.Service.Email.IIdempotencyKeyFactory,
                MiApp.Services.Service.Email.IdempotencyKeyFactory>();
            services.TryAddScoped<
                MiApp.Services.Service.Email.IServiceDespachoCorreo,
                MiApp.Services.Service.Email.ServicioDespachoCorreo>();
        }

        return services;
    }

    private static IServiceCollection AddEmailDispatchRepositories(this IServiceCollection services)
    {
        services.TryAddScoped<MiApp.Repository.Repositorio.Email.IRepositorioCorreo, MiApp.Repository.Repositorio.Email.RepositorioCorreo>();
        services.TryAddScoped<MiApp.Repository.Repositorio.Email.RepositorioCorreoHealthCheck>();

        return services;
    }
}
