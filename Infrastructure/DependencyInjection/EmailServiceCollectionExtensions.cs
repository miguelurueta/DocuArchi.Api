using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DocuArchi.Api.Infrastructure.DependencyInjection;

public static class EmailServiceCollectionExtensions
{
    /// <summary>Conserva la composicion publica del motor Email y registra sus adaptadores y salud pasiva.</summary>
    /// <param name="services">Coleccion de servicios donde se conservan registros previos compatibles.</param>
    /// <param name="configuration">Configuracion central del motor, broker y proveedor ACS.</param>
    /// <returns>La misma coleccion con los servicios del modulo registrados.</returns>
    public static IServiceCollection AddEmailDispatchEngine(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddEmailDispatchRepositories();
        services.AddOptions<MiApp.Services.Service.Email.Queue.EmailQueueOptions>()
            .Bind(configuration.GetSection("EmailDispatch:Broker"))
            .Validate(options => options.IsValid(), "EmailDispatch:Broker no está configurado correctamente.")
            .ValidateOnStart();
        services.TryAddSingleton<MiApp.Services.Service.Email.Queue.EmailWorkerStatus>();
        if (!services.Any(d => d.ServiceType == typeof(MiApp.Services.Service.Email.Queue.EmailWorkerHealthCheck)))
        {
            services.AddScoped<MiApp.Services.Service.Email.Queue.EmailWorkerHealthCheck>();
            services.AddHealthChecks().AddCheck<MiApp.Services.Service.Email.Queue.EmailWorkerHealthCheck>("email-worker");
        }
        services.AddHttpContextAccessor();
        services.AddOptions<MiApp.Services.Service.Email.Attachments.EmailAttachmentOptions>()
            .Bind(configuration.GetSection("EmailDispatch:Attachments"))
            .Validate(o => o.IsValid(), "Email attachment configuration is invalid.").ValidateOnStart();
        services.TryAddScoped<MiApp.Services.Service.Email.Attachments.IEmailAttachmentContextAccessor, HttpEmailAttachmentContextAccessor>();
        services.TryAddScoped<MiApp.Services.Service.Email.Attachments.IEmailAttachmentSource, MiApp.Services.Service.Email.Attachments.DeniedEmailAttachmentSource>();
        services.AddOptions<MiApp.Services.Service.Email.Attachments.EmailAttachmentScannerOptions>()
            .Bind(configuration.GetSection("EmailDispatch:Attachments:Scanner"))
            .Validate(o => o.IsValid(), "Email scanner configuration is invalid.").ValidateOnStart();
        if (configuration.GetValue<string>("EmailDispatch:Attachments:Scanner:Provider") == "ClamD")
            services.TryAddScoped<MiApp.Services.Service.Email.Attachments.IEmailAttachmentScanner, MiApp.Services.Service.Email.Attachments.ClamAvEmailAttachmentScanner>();
        else
            services.TryAddScoped<MiApp.Services.Service.Email.Attachments.IEmailAttachmentScanner, MiApp.Services.Service.Email.Attachments.DeniedEmailAttachmentScanner>();
        services.TryAddScoped<MiApp.Services.Service.Email.Attachments.IEmailAttachmentStore, MiApp.Services.Service.Email.Attachments.FileEmailAttachmentStore>();
        services.AddOptions<MiApp.Services.Service.Email.EmailAcsOptions>()
            .Bind(configuration.GetSection("EmailDispatch:AzureAcs"));
        services.TryAddSingleton<MiApp.Services.Service.Email.Providers.Azure.IAzureCommunicationEmailMapper,
            MiApp.Services.Service.Email.Providers.Azure.AzureCommunicationEmailMapper>();
        if (!services.Any(d => d.ServiceType == typeof(MiApp.Services.Service.Email.Providers.Azure.AzureCommunicationHealthCheck)))
        {
            services.AddScoped<MiApp.Services.Service.Email.Providers.Azure.AzureCommunicationHealthCheck>();
            services.AddHealthChecks().AddCheck<MiApp.Services.Service.Email.Providers.Azure.AzureCommunicationHealthCheck>("azure-email");
        }
        if (configuration.GetValue<bool>("EmailDispatch:Broker:UseAzureServiceBus"))
            services.TryAddSingleton<MiApp.Services.Service.Email.Queue.IMessageBus, MiApp.Services.Service.Email.Queue.AzureServiceBusMessageBus>();
        else
            services.TryAddSingleton<MiApp.Services.Service.Email.Queue.IMessageBus, MiApp.Services.Service.Email.Queue.MessageBus>();
        services.TryAddScoped<MiApp.Services.Service.Email.Queue.IServicioColaCorreo, MiApp.Services.Service.Email.Queue.ServicioColaCorreo>();
        services.TryAddScoped<MiApp.Services.Service.Email.Queue.EmailOutboxRecoveryService>();
        services.TryAddScoped<MiApp.Services.Service.Email.Queue.IProcesadorMensajeCorreo, MiApp.Services.Service.Email.Queue.ProcesadorMensajeCorreo>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<MiApp.Services.Service.Email.IEmailProvider, MiApp.Services.Service.Email.AzureCommunicationServicesEmailProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<MiApp.Services.Service.Email.IEmailProvider, MiApp.Services.Service.Email.CertimailEmailProvider>());
        services.TryAddScoped<MiApp.Services.Service.Email.IEmailProviderResolver, MiApp.Services.Service.Email.EmailProviderResolver>();
        if (configuration.GetValue<bool>("EmailDispatch:Broker:UseAzureServiceBus"))
        {
            services.TryAddScoped<MiApp.Services.Service.Email.Queue.IConsumidorMensajes, MiApp.Services.Service.Email.Queue.AzureServiceBusEmailConsumer>();
            services.AddHostedService<MiApp.Services.Service.Email.Queue.EmailDispatchWorkerHostedService>();
            services.AddHostedService<MiApp.Services.Service.Email.Queue.EmailOutboxRecoveryWorker>();
        }
        else
            services.TryAddScoped<MiApp.Services.Service.Email.Queue.IConsumidorMensajes, MiApp.Services.Service.Email.Queue.NoOpConsumidorMensajes>();
        services.AddOptions<MiApp.Services.Service.Email.EmailDispatchOptions>()
            .Bind(configuration.GetSection("EmailDispatch"));
        services.TryAddScoped<MiApp.Services.Service.Email.IIdempotencyKeyFactory, MiApp.Services.Service.Email.IdempotencyKeyFactory>();
        services.TryAddScoped<MiApp.Services.Service.Email.IEmailRecipientExistenceValidator, MiApp.Services.Service.Email.DisabledEmailRecipientExistenceValidator>();
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
        services.TryAddScoped<MiApp.Services.Service.Email.IEmailRecipientExistenceValidator, MiApp.Services.Service.Email.DisabledEmailRecipientExistenceValidator>();
        services.TryAddScoped<MiApp.Services.Service.Email.IServiceDespachoCorreo, MiApp.Services.Service.Email.ServicioDespachoCorreo>();
        return services;
    }
}
