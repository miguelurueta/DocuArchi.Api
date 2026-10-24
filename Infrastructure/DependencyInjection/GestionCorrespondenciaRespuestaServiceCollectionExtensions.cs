using MiApp.Repository.Repositorio.GestionCorrespondencia.GestionRespuesta.Core;
using MiApp.Services.Service.GestionCorrespondencia.GestionRespuesta.Core;
using MiApp.Repository.Repositorio.GestionCorrespondencia.GestionRespuesta.Core.Preparacion;
using MiApp.Services.Service.GestionCorrespondencia.GestionRespuesta.Core.Preparacion;
using DocuArchi.Api.Infrastructure.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

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
            services.AddScoped<IPreparacionRespuestaRepository, PreparacionRespuestaRepository>();
            services.AddScoped<IPoliticaPreparacionRespuesta, PoliticaPreparacionRespuesta>();
            services.AddScoped<IPreparacionRespuestaService, PreparacionRespuestaService>();
            services.Configure<SwaggerGenOptions>(options =>
            {
                options.OperationFilter<PreparacionRespuestaOperationFilter>();
                options.DocumentFilter<PreparacionRespuestaSecurityFilter>();
            });

            return services;
        }
    }
}
