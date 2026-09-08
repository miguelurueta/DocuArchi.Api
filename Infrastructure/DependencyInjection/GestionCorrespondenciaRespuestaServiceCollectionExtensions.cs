using MiApp.Repository.Repositorio.GestionCorrespondencia.GestionRespuesta.Core;
using MiApp.Services.Service.GestionCorrespondencia.GestionRespuesta.Core;

namespace DocuArchi.Api.Infrastructure.DependencyInjection
{
    public static class GestionCorrespondenciaRespuestaServiceCollectionExtensions
    {
        public static IServiceCollection AddGestionCorrespondenciaRespuesta(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<ISolicitaDocumentosAdjuntosRespuestaRadicadoRepository, SolicitaDocumentosAdjuntosRespuestaRadicadoRepository>();
            services.AddScoped<IServiceSolicitaDocumentosAdjuntosRespuestaRadicado, ServiceSolicitaDocumentosAdjuntosRespuestaRadicado>();

            return services;
        }
    }
}
