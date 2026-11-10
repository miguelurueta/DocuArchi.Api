using MiApp.Repository.Repositorio.GestionCorrespondencia.GestionRespuesta.Core;
using MiApp.Services.Service.GestionCorrespondencia.GestionRespuesta.Core;
using MiApp.Repository.Repositorio.GestionCorrespondencia.GestionRespuesta.Core.Preparacion;
using MiApp.Services.Service.GestionCorrespondencia.GestionRespuesta.Core.Preparacion;
using DocuArchi.Api.Infrastructure.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MiApp.Repository.Repositorio.Workflow.Terminar;

namespace DocuArchi.Api.Infrastructure.DependencyInjection
{
    public static class GestionCorrespondenciaRespuestaServiceCollectionExtensions
    {
        /// <summary>Registra preparación y sus lectores Workflow reutilizados sin imponer orden entre módulos.</summary>
        /// <param name="services">Colección de servicios del host.</param>
        /// <param name="configuration">Configuración del módulo, conservada por compatibilidad.</param>
        /// <returns>La colección con servicios scoped y configuración Swagger del módulo.</returns>
        public static IServiceCollection AddGestionCorrespondenciaRespuesta(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<ISolicitaDocumentosAdjuntosRespuestaRadicadoRepository, SolicitaDocumentosAdjuntosRespuestaRadicadoRepository>();
            services.AddScoped<IServiceSolicitaDocumentosAdjuntosRespuestaRadicado, ServiceSolicitaDocumentosAdjuntosRespuestaRadicado>();
            services.AddScoped<IPreparacionRespuestaRepository, PreparacionRespuestaRepository>();
            services.AddScoped<IPoliticaPreparacionRespuesta, PoliticaPreparacionRespuesta>();
            services.TryAddScoped<IWorkflowPreviewIdentityRepository, WorkflowPreviewIdentityRepository>();
            services.TryAddScoped<IWorkflowTaskPreviewRepository, WorkflowTaskPreviewRepository>();
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
