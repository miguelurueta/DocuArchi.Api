using MiApp.Repository.Repositorio.DocumentProcessing.Core;
using MiApp.Services.Service.DocumentProcessing.Core;
using MiApp.Services.Service.DocumentProcessing.Ocr.Workers;
using DocuArchi.Api.Infrastructure.HostedServices;

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

            services.AddScoped<IDocumentProcessingRepository, DocumentProcessingRepository>();
            services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();
            services.AddScoped<IDocumentProcessingSourceResolver, TemporaryUploadDocumentProcessingSourceResolver>();
            services.AddScoped<IDocumentProcessingSourceResolver, StoredDocumentProcessingSourceResolver>();
            services.AddScoped<IDocumentProcessingSourceResolver, PhysicalPathDocumentProcessingSourceResolver>();
            services.AddScoped<IDocumentProcessingStoredDocumentResolver, NotConfiguredStoredDocumentResolver>();
            services.AddScoped<IOcrDocumentProcessorAdapter, OcrDocumentProcessorAdapter>();
            services.AddScoped<IOcrArtifactReflectionService, OcrArtifactReflectionService>();
            services.AddScoped<IDocumentProcessingOcrWorkerCoordinator, DocumentProcessingOcrWorkerCoordinator>();
            services.AddScoped<IOcrJobProcessor, DocumentProcessingOcrJobProcessor>();
            services.AddHostedService<OcrWorkerHostedService>();

            return services;
        }
    }
}
