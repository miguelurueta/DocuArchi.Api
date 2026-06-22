using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DocuArchi.Api.Infrastructure.DependencyInjection
{
    public static class RespuestaHistoricalContextServiceCollectionExtensions
    {
        public static IServiceCollection AddRespuestaHistoricalContext(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<MiApp.Services.Service.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.IRespuestaHistoricalContextService, MiApp.Services.Service.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.RespuestaHistoricalContextService>();
            services.AddScoped<MiApp.Repository.Repositorio.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.IRespuestaHistoricalContextReadRepository, MiApp.Repository.Repositorio.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.RespuestaHistoricalContextReadRepository>();

            return services;
        }
    }
}
