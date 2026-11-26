using MiApp.Repository.Repositorio.GestorDocumental.ContextoDocumental;
using MiApp.Repository.Repositorio.GestorDocumental.Documentos.ListaDocumentosRadicados;
using MiApp.Repository.Repositorio.GestorDocumental.Editor;
using MiApp.Repository.Repositorio.GestorDocumental.TipologiasDocumentales;
using MiApp.Services.Service.GestorDocumental.ContextoDocumental;
using MiApp.Services.Service.GestorDocumental.Documentos.ListaDocumentosRadicados;
using MiApp.Services.Service.GestorDocumental.Editor;
using MiApp.Services.Service.GestorDocumental.TipologiasDocumentales;
using MiApp.Services.Service.Workflow.BandejaCorrespondencia;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DocuArchi.Api.Infrastructure.DependencyInjection
{
    public static class GestorDocumentalServiceCollectionExtensions
    {
        /// <summary>Registra los servicios del módulo documental y la actualización de tipología, conservando sus componentes existentes.</summary>
        /// <param name="services">Contenedor de la API que recibe los registros del módulo.</param>
        /// <param name="configuration">Configuración recibida por el contrato existente; no se modifica.</param>
        /// <returns>Misma colección con los componentes documentales registrados.</returns>
        /// <remarks>Nombre público compatible usado por Program.cs; excepción LEG-07, sin renombrado general.</remarks>
        public static IServiceCollection AddGestorDocumental(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddTipologiasDocumentales();
            services.AgregarActualizacionTipologiaDocumental();
            services.RegistrarContextoDocumental();
            services.AddListaDocumentosRadicados();
            services.AddEditorImageContent();

            return services;
        }

        private static IServiceCollection AddEditorImageContent(this IServiceCollection services)
        {
            services.AddScoped<IGuardaEditorImageRepository, GuardaEditorImageRepository>();
            services.AddScoped<IServiceGuardaEditorImage, ServiceGuardaEditorImage>();
            services.AddScoped<ISolicitaEditorImageContentRepository, SolicitaEditorImageContentRepository>();
            services.AddScoped<IServiceSolicitaEditorImageContent, ServiceSolicitaEditorImageContent>();

            return services;
        }

        private static IServiceCollection AddListaDocumentosRadicados(this IServiceCollection services)
        {
            services.AddScoped<IListaDocumentosRadicadosRepository, ListaDocumentosRadicadosRepository>();
            services.AddScoped<IListaDocumentosRadicadoService, ListaDocumentosRadicadoService>();
            services.AddScoped<IListaDocumentosRadicadosOperacionalService, ListaDocumentosRadicadosOperacionalService>();

            return services;
        }

        /// <summary>Registra lectores y resolutores por origen, conservando los ejecutores de módulos fuera del listado.</summary>
        /// <param name="services">Contenedor modular de la API.</param>
        /// <returns>El mismo contenedor con resolutores de contexto únicos por origen.</returns>
        private static IServiceCollection RegistrarContextoDocumental(this IServiceCollection services)
        {
            services.AddScoped<IContextoDocumentalRepository, ContextoDocumentalRepository>();
            services.AddScoped<IContextoDocumentalResolver, RadicacionContextResolver>();
            services.AddScoped<IContextoDocumentalResolver, WorkflowContextResolver>();
            services.AddScoped<IReferenciasContextoListadoRepository, ReferenciasContextoListadoRepository>();
            services.AddScoped<IContextoDocumentalResolver, ProduccionListadoContextResolver>();
            services.AddScoped<IContextoDocumentalResolver, DocuarchiListadoContextResolver>();
            services.AddScoped<IContextoDocumentalResolver, MigracionListadoContextResolver>();
            services.AddScoped<IContextoDocumentalRadicadoService, ContextoDocumentalRadicadoService>();
            services.AddScoped<IWorkflowInboxContextResolverService, WorkflowInboxContextResolverService>();

            return services;
        }

        private static IServiceCollection AddTipologiasDocumentales(this IServiceCollection services)
        {
            services.AddScoped<IServiceSolicitaListaTipologiasDocumentales, ServiceSolicitaListaTipologiasDocumentales>();
            services.AddScoped<ISolicitaListaTipologiasDocumentalesRepository, SolicitaListaTipologiasDocumentalesRepository>();
            services.AddScoped<IContextoTipologiaResolverService, ContextoTipologiaResolverService>();
            services.AddScoped<IContextoTipologiaResolverRepository, SolicitaListaTipologiasDocumentalesRepository>();
            services.AddScoped<ITipologiasDocumentalesContextCoordinator, TipologiasDocumentalesContextCoordinator>();
            services.AddScoped<ITipologiasDocumentalesContextStrategy, RadicacionTipologiasContextStrategy>();
            services.AddScoped<ITipologiasDocumentalesContextStrategy, WorkflowTipologiasContextStrategy>();
            services.AddScoped<ITipologiasDocumentalesContextStrategy, GestionRespuestaTipologiasContextStrategy>();
            services.AddScoped<ITipologiasDocumentalesContextStrategy, ProduccionTipologiasContextStrategy>();
            services.AddScoped<ITipologiasDocumentalesContextStrategy, TrdSerieTipologiasContextStrategy>();
            services.AddScoped<ITipologiasDocumentalesContextStrategy, TrdSubSerieTipologiasContextStrategy>();
            services.AddScoped<ITipologiasDocumentalesContextStrategy, MigracionTipologiasContextStrategy>();

            return services;
        }
    }
}
