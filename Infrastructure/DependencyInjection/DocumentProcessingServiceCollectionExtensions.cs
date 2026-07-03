using MiApp.Repository.Repositorio.DocumentProcessing.Core;
using MiApp.Repository.Repositorio.DocumentProcessing.Ocr.Configuration;
using MiApp.Repository.Repositorio.DocumentProcessing.Office.Configuration;
using MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.StorageRoute;
using MiApp.Repository.Repositorio.GestorDocumental.Documentos.ReemplazoPdf;
using MiApp.Services.Service.DocumentProcessing.Core;
using MiApp.Services.Service.DocumentProcessing.Ocr.Artifacts;
using MiApp.Services.Service.DocumentProcessing.Office.Artifacts;
using MiApp.Services.Service.DocumentProcessing.Office.Conversion;
using MiApp.Services.Service.DocumentProcessing.Office.Configuration;
using MiApp.Services.Service.DocumentProcessing.Office.Extraction;
using MiApp.Services.Service.DocumentProcessing.Office.Quality;
using MiApp.Services.Service.DocumentProcessing.Ocr.Workers;
using MiApp.Services.Service.DocumentProcessing.Office.Workers;
using DocuArchi.Api.Infrastructure.HostedServices;
using MiApp.Services.Service.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.DocumentExtraction.Ocr;
using MiApp.Services.Service.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.DocumentExtraction.Office;
using MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Physical;

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
            services.Configure<DocumentProcessingOfficeWorkerOptions>(
                configuration.GetSection("DocumentProcessing:OfficeWorker"));
            services.Configure<RespuestaHistoricalOcrOptions>(
                configuration.GetSection("RespuestaHistoricalContext:DocumentExtraction:Ocr"));
            services.Configure<RespuestaHistoricalOfficeExtractionOptions>(
                configuration.GetSection("RespuestaHistoricalContext:DocumentExtraction:Office"));

            services.AddScoped<IDocumentProcessingRepository, DocumentProcessingRepository>();
            services.AddScoped<IOfficeConfigurationRepository, OfficeConfigurationRepository>();
            services.AddScoped<IOfficeJobRepository, OfficeJobRepository>();
            services.AddScoped<IOfficeArtifactRepository, OfficeArtifactRepository>();
            services.AddScoped<IOfficeTextChunkRepository, OfficeTextChunkRepository>();
            services.AddScoped<IOfficeConfigurationResolver, OfficeConfigurationResolver>();
            services.AddScoped<IRespuestaHistoricalLegacyDocConverter, RespuestaHistoricalLibreOfficeDocConverter>();
            services.AddScoped<IOfficeLegacyConverter, OfficeLegacyConverter>();
            services.AddScoped<IOfficeTextExtractor, OfficeTextExtractor>();
            services.AddScoped<IOfficeTextQualityEvaluator, OfficeTextQualityEvaluator>();
            services.AddScoped<IOfficeArtifactStore, OfficeArtifactStore>();
            services.AddScoped<IOfficeTextChunkService, OfficeTextChunkService>();
            services.AddScoped<IOfficeDocumentConverter, LibreOfficeDocumentConverter>();
            services.AddScoped<IOfficePdfaValidator, ExternalProcessOfficePdfaValidator>();
            services.AddScoped<IOfficeDocumentConversionService, OfficeDocumentConversionService>();
            services.AddScoped<IOfficeRetentionPolicyResolver, OfficeRetentionPolicyResolver>();
            services.AddScoped<IOfficeExtractionService, OfficeExtractionService>();
            services.AddScoped<IOcrArtifactRepository, OcrArtifactRepository>();
            services.AddScoped<IOcrTextChunkRepository, OcrTextChunkRepository>();
            services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();
            services.AddScoped<IDocumentProcessingSourceResolver, TemporaryUploadDocumentProcessingSourceResolver>();
            services.AddScoped<IDocumentProcessingSourceResolver, StoredDocumentProcessingSourceResolver>();
            services.AddScoped<IDocumentProcessingSourceResolver, PhysicalPathDocumentProcessingSourceResolver>();
            services.AddScoped<IReemplazoPdfDocumentLocationRepository, ReemplazoPdfDocumentLocationRepository>();
            services.AddScoped<IStorageRouteRepository, StorageRouteRepository>();
            services.AddScoped<IStorageFolderLegacyPolicy, StorageFolderLegacyPolicy>();
            services.AddScoped<IStoragePathResolver, StoragePathResolver>();
            services.AddScoped<IDocumentProcessingStoredDocumentResolver, StoredDocumentDocumentProcessingResolver>();
            services.AddScoped<IDocumentProcessingOcrTextReadService, DocumentProcessingOcrTextReadService>();
            services.AddScoped<IOcrDocumentProcessorAdapter, OcrDocumentProcessorAdapter>();
            services.AddScoped<IOfficeDocumentProcessorAdapter, OfficeDocumentProcessorAdapter>();
            services.AddScoped<IOcrArtifactReflectionService, OcrArtifactReflectionService>();
            services.AddScoped<IOfficeArtifactReflectionService, OfficeArtifactReflectionService>();
            services.AddScoped<IDocumentProcessingOcrWorkerCoordinator, DocumentProcessingOcrWorkerCoordinator>();
            services.AddScoped<IDocumentProcessingOfficeWorkerCoordinator, DocumentProcessingOfficeWorkerCoordinator>();
            services.AddScoped<IOcrJobProcessor, DocumentProcessingOcrJobProcessor>();
            services.AddScoped<IOfficeJobQueueService, OfficeJobQueueService>();
            services.AddScoped<IOfficeJobWorker, OfficeJobWorker>();
            services.AddScoped<IOfficeJobProcessor, OfficeJobProcessor>();
            services.AddScoped<IOfficeOperationalReportService, OfficeOperationalReportService>();
            services.AddHostedService<OcrWorkerHostedService>();
            services.AddHostedService<OfficeWorkerHostedService>();

            return services;
        }
    }
}
