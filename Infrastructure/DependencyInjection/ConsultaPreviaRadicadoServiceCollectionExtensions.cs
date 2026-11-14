using MiApp.Repository.Repositorio.Radicador.ResolucionDocumental;
using MiApp.Services.Service.Radicacion.ResolucionDocumental;
using MiApp.Repository.Repositorio.Radicador.ConsultaPrevia.Acciones;
using MiApp.Repository.Repositorio.Radicador.ConsultaPrevia.Autocomplete;
using MiApp.Repository.Repositorio.Radicador.ConsultaPrevia.Estructura;
using MiApp.Repository.Repositorio.Radicador.ConsultaPrevia.Query;
using MiApp.Services.Service.Radicacion.ConsultaPrevia.Acciones;
using MiApp.Services.Service.Radicacion.ConsultaPrevia.Autocomplete;
using MiApp.Services.Service.Radicacion.ConsultaPrevia.Core;
using MiApp.Services.Service.Radicacion.ConsultaPrevia.Detalle;
using MiApp.Services.Service.Radicacion.ConsultaPrevia.Errors;
using MiApp.Services.Service.Radicacion.ConsultaPrevia.Estructura;
using MiApp.Services.Service.Radicacion.ConsultaPrevia.Fields;
using MiApp.Services.Service.Radicacion.ConsultaPrevia.Query;

namespace DocuArchi.Api.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Registra la composicion de Consulta Previa de Radicado.
    /// </summary>
    public static class ConsultaPreviaRadicadoServiceCollectionExtensions
    {
        /// <summary>Registra la capacidad documental compartida junto a los servicios existentes de Consulta Previa.</summary>
        /// <param name="services">Contenedor que recibe la capacidad documental y la composición existente.</param>
        /// <param name="configuration">Configuración conservada por compatibilidad con las llamadas existentes.</param>
        /// <returns>El mismo contenedor con servicios y repositorios de Consulta Previa registrados.</returns>
        public static IServiceCollection AddConsultaPreviaRadicado(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<IResolucionDocumentoRadicacionRepository, ResolucionDocumentoRadicacionRepository>();
            services.AddScoped<IResolucionDocumentoRadicacionService, ResolucionDocumentoRadicacionService>();
            services.AddScoped<ICatalogoSeleccionConsultaPreviaRepository, CatalogoSeleccionConsultaPreviaRepository>();
            services.AddScoped<ISeleccionConsultaPreviaService, SeleccionConsultaPreviaService>();
            services.AddScoped<IConsultaPreviaRadicadoEstructuraRepository, ConsultaPreviaRadicadoEstructuraRepository>();
            services.AddScoped<IFuenteDocumentalConsultaPreviaRepository, FuenteDocumentalConsultaPreviaRepository>();
            services.AddScoped<IConsultaPreviaRadicadoRepository, ConsultaPreviaRadicadoRepository>();
            services.AddScoped<IConsultaPreviaRadicadoAutocompleteRepository, ConsultaPreviaRadicadoAutocompleteRepository>();
            services.AddScoped<IConsultaPreviaRadicadoActionRepository, ConsultaPreviaRadicadoActionRepository>();
            services.AddScoped<IConsultaPreviaRadicadoEstructuraService, ConsultaPreviaRadicadoEstructuraService>();
            services.AddScoped<IConsultaPreviaRadicadoFieldResolver, ConsultaPreviaRadicadoFieldResolver>();
            services.AddScoped<IConsultaPreviaRadicadoAutocompleteCatalog, ConsultaPreviaRadicadoAutocompleteCatalog>();
            services.AddScoped<IConsultaPreviaRadicadoAutocompleteProvider, ConsultaPreviaRadicadoDistinctValuesProvider>();
            services.AddScoped<IConsultaPreviaRadicadoAutocompleteProvider, ConsultaPreviaRadicadoNumberProvider>();
            services.AddScoped<IConsultaPreviaRadicadoAutocompleteProvider, ConsultaPreviaRadicadoRemitenteProvider>();
            services.AddScoped<IConsultaPreviaRadicadoAutocompleteProvider, ConsultaPreviaRadicadoDestinatarioProvider>();
            services.AddScoped<IConsultaPreviaRadicadoAutocompleteService, ConsultaPreviaRadicadoAutocompleteService>();
            services.AddScoped<IConsultaPreviaRadicadoQueryService, ConsultaPreviaRadicadoQueryService>();
            services.AddScoped<IConsultaPreviaRadicadoDetalleService, ConsultaPreviaRadicadoDetalleService>();
            services.AddScoped<IConsultaPreviaRadicadoActionService, ConsultaPreviaRadicadoActionService>();
            services.AddScoped<IConsultaPreviaRadicadoService, ConsultaPreviaRadicadoService>();
            services.AddScoped<ConsultaPreviaRadicadoErrorResponseFactory>();

            return services;
        }
    }
}
