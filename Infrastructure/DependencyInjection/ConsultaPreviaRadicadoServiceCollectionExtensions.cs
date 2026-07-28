using MiApp.Repository.Repositorio.Radicador.ConsultaPrevia.Estructura;
using MiApp.Repository.Repositorio.Radicador.ConsultaPrevia.Query;
using MiApp.Services.Service.Radicacion.ConsultaPrevia.Core;
using MiApp.Services.Service.Radicacion.ConsultaPrevia.Errors;
using MiApp.Services.Service.Radicacion.ConsultaPrevia.Estructura;
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
            services.AddScoped<IConsultaPreviaRadicadoEstructuraService, ConsultaPreviaRadicadoEstructuraService>();
            services.AddScoped<IConsultaPreviaRadicadoQueryService, ConsultaPreviaRadicadoQueryService>();
            services.AddScoped<IConsultaPreviaRadicadoService, ConsultaPreviaRadicadoService>();
            services.AddScoped<ConsultaPreviaRadicadoErrorResponseFactory>();

            return services;
        }
    }
}
