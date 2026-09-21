using MiApp.Repository.Repositorio.GestorDocumental.ContextoDocumental;
using MiApp.Repository.Repositorio.GestorDocumental.Documentos.ListaDocumentosRadicados;
using MiApp.Repository.Repositorio.GestorDocumental.TipologiasDocumentales;
using MiApp.Services.Service.GestorDocumental.ContextoDocumental;
using MiApp.Services.Service.GestorDocumental.Documentos.ListaDocumentosRadicados;
using MiApp.Services.Service.GestorDocumental.TipologiasDocumentales;
using MiApp.Services.Service.Workflow.BandejaCorrespondencia;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DocuArchi.Api.Infrastructure.DependencyInjection
{
    public static class GestorDocumentalServiceCollectionExtensions
    {
        public static IServiceCollection AddGestorDocumental(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddTipologiasDocumentales();
            services.AddContextoDocumental();
            services.AddListaDocumentosRadicados();

            return services;
        }

        private static IServiceCollection AddListaDocumentosRadicados(this IServiceCollection services)
        {
            services.AddScoped<IListaDocumentosRadicadosRepository, ListaDocumentosRadicadosRepository>();
            services.AddScoped<IListaDocumentosRadicadoService, ListaDocumentosRadicadoService>();
            services.AddScoped<IListaDocumentosRadicadosOperacionalService, ListaDocumentosRadicadosOperacionalService>();

            return services;
        }

        private static IServiceCollection AddContextoDocumental(this IServiceCollection services)
        {
            services.AddScoped<IContextoDocumentalRepository, ContextoDocumentalRepository>();
            services.AddScoped<IContextoDocumentalResolver, RadicacionContextResolver>();
            services.AddScoped<IContextoDocumentalResolver, WorkflowContextResolver>();
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
