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
            services.AddScoped<MiApp.Services.Service.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.Consulta.IRespuestaHistoricalContextService, MiApp.Services.Service.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.Consulta.RespuestaHistoricalContextService>();
            services.AddScoped<MiApp.Services.Service.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.Indexacion.IRespuestaHistoricalContextIndexacionAdminService, MiApp.Services.Service.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.Indexacion.RespuestaHistoricalContextIndexacionAdminService>();
            services.AddScoped<MiApp.Services.Service.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.Backfill.IRespuestaHistoricalBackfillService, MiApp.Services.Service.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.Backfill.RespuestaHistoricalBackfillService>();
            services.AddScoped<MiApp.Services.Service.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.Incremental.IRespuestaHistoricalIncrementalService, MiApp.Services.Service.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.Incremental.RespuestaHistoricalIncrementalService>();
            services.AddScoped<MiApp.Repository.Repositorio.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.IRespuestaHistoricalContextReadRepository, MiApp.Repository.Repositorio.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.RespuestaHistoricalContextReadRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.IRespuestaHistoricalBackfillSourceRepository, MiApp.Repository.Repositorio.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.RespuestaHistoricalBackfillSourceRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.IRespuestaHistoricalIncrementalSourceRepository, MiApp.Repository.Repositorio.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.RespuestaHistoricalIncrementalSourceRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.IRespuestaHistoricalContextIndexRepository, MiApp.Repository.Repositorio.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.RespuestaHistoricalContextIndexRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.IRespuestaHistoricalDocumentIndexRepository, MiApp.Repository.Repositorio.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.RespuestaHistoricalDocumentIndexRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.IRespuestaHistoricalContextEventRepository, MiApp.Repository.Repositorio.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.RespuestaHistoricalContextEventRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.IRespuestaHistoricalIndexOperationRepository, MiApp.Repository.Repositorio.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.RespuestaHistoricalIndexOperationRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.IRespuestaHistoricalContextCheckpointRepository, MiApp.Repository.Repositorio.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.RespuestaHistoricalContextCheckpointRepository>();

            return services;
        }
    }
}
