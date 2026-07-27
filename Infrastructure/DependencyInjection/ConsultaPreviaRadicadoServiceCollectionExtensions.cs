using MiApp.Repository.Repositorio.Radicador.ConsultaPrevia.Estructura;
using MiApp.Services.Service.Radicacion.ConsultaPrevia.Core;
using MiApp.Services.Service.Radicacion.ConsultaPrevia.Errors;
using MiApp.Services.Service.Radicacion.ConsultaPrevia.Estructura;

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
            services.AddScoped<IConsultaPreviaRadicadoEstructuraService, ConsultaPreviaRadicadoEstructuraService>();
            services.AddScoped<IConsultaPreviaRadicadoService, ConsultaPreviaRadicadoService>();
            services.AddScoped<ConsultaPreviaRadicadoErrorResponseFactory>();

            return services;
        }
    }
}
