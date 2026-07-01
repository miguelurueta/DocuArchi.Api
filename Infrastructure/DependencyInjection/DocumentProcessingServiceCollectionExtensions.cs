using MiApp.Repository.Repositorio.DocumentProcessing.Core;
using MiApp.Repository.Repositorio.DocumentProcessing.Ocr.Configuration;
using MiApp.Services.Service.DocumentProcessing.Core;
using MiApp.Services.Service.DocumentProcessing.Ocr.Artifacts;
using MiApp.Services.Service.DocumentProcessing.Ocr.Workers;
using DocuArchi.Api.Infrastructure.HostedServices;
using MiApp.Services.Service.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.DocumentExtraction.Ocr;

namespace DocuArchi.Api.Infrastructure.DependencyInjection
{
    public static class DocumentProcessingServiceCollectionExtensions
    {
        public static IServiceCollection AddDocumentProcessing(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var workerSection = configuration.GetSection("DocumentProcessing:OcrWorker");
            services.Configure<DocumentProcessingOcrWorkerOptions>(workerSection);
            services.Configure<RespuestaHistoricalOcrOptions>(
                configuration.GetSection("RespuestaHistoricalContext:DocumentExtraction:Ocr"));

            services.AddScoped<IDocumentProcessingRepository, DocumentProcessingRepository>();
            services.AddScoped<IOcrArtifactRepository, OcrArtifactRepository>();
            services.AddScoped<IOcrTextChunkRepository, OcrTextChunkRepository>();
            services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();
            services.AddScoped<IDocumentProcessingSourceResolver, TemporaryUploadDocumentProcessingSourceResolver>();
            services.AddScoped<IDocumentProcessingSourceResolver, StoredDocumentProcessingSourceResolver>();
            services.AddScoped<IDocumentProcessingSourceResolver, PhysicalPathDocumentProcessingSourceResolver>();
            services.AddScoped<IDocumentProcessingStoredDocumentResolver, NotConfiguredStoredDocumentResolver>();
            services.AddScoped<IDocumentProcessingOcrTextReadService, DocumentProcessingOcrTextReadService>();
            services.AddScoped<IOcrDocumentProcessorAdapter, OcrDocumentProcessorAdapter>();
            services.AddScoped<IOcrArtifactReflectionService, OcrArtifactReflectionService>();
            services.AddScoped<IDocumentProcessingOcrWorkerCoordinator, DocumentProcessingOcrWorkerCoordinator>();
            services.AddScoped<IOcrJobProcessor, DocumentProcessingOcrJobProcessor>();
            services.AddHostedService<OcrWorkerHostedService>();

            return services;
        }
    }
}
