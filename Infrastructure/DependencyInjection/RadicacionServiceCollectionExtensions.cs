using MiApp.Services.Service.Radicacion.Tramite.Configuracion;

namespace DocuArchi.Api.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Registra politicas y servicios transversales de radicacion.
    /// </summary>
    public static class RadicacionServiceCollectionExtensions
    {
        public static IServiceCollection AddRadicacionCore(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<RadicacionCamposFijosOptions>(
                configuration.GetSection("Radicacion:CamposFijos"));
            services.AddScoped<IRadicacionCampoFijoPolicyService, RadicacionCampoFijoPolicyService>();

            return services;
        }
    }
}
