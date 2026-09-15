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
        services.TryAddSingleton<MiApp.Services.Service.Email.Queue.IMessageBus, MiApp.Services.Service.Email.Queue.MessageBus>();
        services.TryAddScoped<MiApp.Services.Service.Email.Queue.IServicioColaCorreo, MiApp.Services.Service.Email.Queue.ServicioColaCorreo>();
        services.AddOptions<MiApp.Services.Service.Email.EmailDispatchOptions>()
            .Bind(configuration.GetSection("EmailDispatch"));
        services.TryAddScoped<MiApp.Services.Service.Email.IIdempotencyKeyFactory, MiApp.Services.Service.Email.IdempotencyKeyFactory>();
        services.TryAddScoped<MiApp.Services.Service.Email.IServiceDespachoCorreo, MiApp.Services.Service.Email.ServicioDespachoCorreo>();
        services.TryAddScoped<MiApp.Services.Service.Email.IServicioCorreo, MiApp.Services.Service.Email.ServicioCorreo>();

        // EMAIL-05 must register a real IServicioColaCorreo before enabling dispatch.
        if (configuration.GetValue<bool>("EmailDispatch:EnableDispatchService"))
            services.AddEmailDispatchService(configuration);

        return services;
    }

    private static IServiceCollection AddEmailDispatchRepositories(this IServiceCollection services)
    {
        services.TryAddScoped<MiApp.Repository.Repositorio.Email.IRepositorioCorreo, MiApp.Repository.Repositorio.Email.RepositorioCorreo>();
        services.TryAddScoped<MiApp.Repository.Repositorio.Email.RepositorioCorreoHealthCheck>();

        return services;
    }

    public static IServiceCollection AddEmailDispatchService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEmailDispatchRepositories();
        services.AddOptions<MiApp.Services.Service.Email.EmailDispatchOptions>()
            .Bind(configuration.GetSection("EmailDispatch"))
            .Validate(options => options.IsValid(), "EmailDispatch contains invalid limits.")
            .ValidateOnStart();
        services.TryAddScoped<MiApp.Services.Service.Email.IIdempotencyKeyFactory, MiApp.Services.Service.Email.IdempotencyKeyFactory>();
        services.TryAddScoped<MiApp.Services.Service.Email.IServiceDespachoCorreo, MiApp.Services.Service.Email.ServicioDespachoCorreo>();
        return services;
    }
}
