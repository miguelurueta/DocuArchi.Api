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
        public static IServiceCollection AddConsultaPreviaRadicado(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<IConsultaPreviaRadicadoEstructuraRepository, ConsultaPreviaRadicadoEstructuraRepository>();
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
