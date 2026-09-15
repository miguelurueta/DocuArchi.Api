using MiApp.Services.Service.Radicacion.Tramite.Configuracion;
using MiApp.Services.Service.Radicacion.PlantillaRadicado;
using MiApp.Services.Service.Radicacion.PlantillaValidacion;

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
            services.AddScoped<IAsignacionCamposValidacionDinamicaService, AsignacionCamposValidacionDinamicaService>();
            services.AddScoped<IPlantillaRadicacionDefaultService, PlantillaRadicacionDefaultService>();

            return services;
        }
    }
}
