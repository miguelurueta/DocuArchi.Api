using MiApp.Services.Service.Radicacion.Tramite.Configuracion;
using MiApp.Services.Service.Radicacion.PlantillaRadicado;
using MiApp.Services.Service.Radicacion.PlantillaValidacion;
using DocuArchi.Api.Infrastructure.Filters;
using DocuArchi.Api.Infrastructure.Observability;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DocuArchi.Api.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Registra politicas y servicios transversales de radicacion.
    /// </summary>
    public static class RadicacionServiceCollectionExtensions
    {
        /// <summary>Compone políticas de radicación y la correlación segura de errores del selector en las rutas existentes.</summary>
        /// <param name="services">Colección de servicios de la aplicación.</param>
        /// <param name="configuration">Configuración existente de campos fijos de radicación.</param>
        /// <returns>La misma colección con composición modular y filtro de diagnóstico registrados.</returns>
        public static IServiceCollection AddRadicacionCore(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<RadicacionCamposFijosOptions>(
                configuration.GetSection("Radicacion:CamposFijos"));
            services.AddScoped<IRadicacionCampoFijoPolicyService, RadicacionCampoFijoPolicyService>();
            services.AddScoped<IAsignacionCamposValidacionDinamicaService, AsignacionCamposValidacionDinamicaService>();
            services.AddScoped<IPlantillaRadicacionDefaultService, PlantillaRadicacionDefaultService>();
            services.AddHttpContextAccessor();
            services.AddLogging();
            services.TryAddScoped<IRequestCorrelationContext, RequestCorrelationContext>();
            services.TryAddScoped<FiltroDiagnosticoSeleccionPlantilla>();
            services.Configure<MvcOptions>(options =>
            {
                if (!options.Filters.OfType<ServiceFilterAttribute>().Any(filter => filter.ServiceType == typeof(FiltroDiagnosticoSeleccionPlantilla)))
                    options.Filters.AddService<FiltroDiagnosticoSeleccionPlantilla>();
            });

            return services;
        }
    }
}
