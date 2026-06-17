using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DocuArchi.Api.Infrastructure.DependencyInjection
{
    public static class StorageServiceCollectionExtensions
    {
        public static IServiceCollection AddStorageEngine(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddStorageOptions(configuration);
            services.AddStorageDelete();
            services.AddStorageServices();
            services.AddStorageRepositories();
            services.AddStorageTemporaryUpload();
            services.AddStorageCabinetIndex();
            services.AddStorageHostedServices();

            return services;
        }

        private static IServiceCollection AddStorageOptions(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<MiApp.Models.Models.GestorDocumental.AlmacenamientoDocumental.Options.StoragePathOptions>(configuration.GetSection("StoragePaths"));
            services.Configure<MiApp.Models.Models.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload.StorageUploadOptions>(configuration.GetSection("StorageUpload"));
            services.Configure<MiApp.Models.Models.GestorDocumental.AlmacenamientoDocumental.Options.StorageMetadataOptions>(configuration.GetSection("StorageMetadata"));
            services.Configure<MiApp.Models.Models.GestorDocumental.AlmacenamientoDocumental.CabinetIndexProviders.CabinetIndexProviderCatalogOptions>(configuration.GetSection("StorageCabinetIndexProviders"));

            return services;
        }

        private static IServiceCollection AddStorageDelete(this IServiceCollection services)
        {
            services.AddScoped<MiApp.Services.Service.GestorDocumental.EliminarDocumento.IEliminarDocumentoUseCase, MiApp.Services.Service.GestorDocumental.EliminarDocumento.EliminarDocumentoUseCase>();
            services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.EliminarDocumento.IStorageDeleteLookupRepository, MiApp.Repository.Repositorio.GestorDocumental.EliminarDocumento.StorageDeleteLookupRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.EliminarDocumento.IStorageDeleteWriteRepository, MiApp.Repository.Repositorio.GestorDocumental.EliminarDocumento.StorageDeleteWriteRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.EliminarDocumento.IWorkflowDeleteRelationRepository, MiApp.Repository.Repositorio.GestorDocumental.EliminarDocumento.WorkflowDeleteRelationRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.EliminarDocumento.IStorageDeleteCompensationRepository, MiApp.Repository.Repositorio.GestorDocumental.EliminarDocumento.StorageDeleteCompensationRepository>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.EliminarDocumento.Transaction.IStorageDeleteTransactionCoordinator, MiApp.Services.Service.GestorDocumental.EliminarDocumento.Transaction.StorageDeleteTransactionCoordinator>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.EliminarDocumento.Orchestrator.IStorageDeleteOrchestrator, MiApp.Services.Service.GestorDocumental.EliminarDocumento.Orchestrator.StorageDeleteOrchestrator>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.EliminarDocumento.Physical.IStorageDeletePhysicalExecutor, MiApp.Services.Service.GestorDocumental.EliminarDocumento.Physical.StorageDeletePhysicalExecutor>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.EliminarDocumento.Physical.IStorageDeleteCompensationService, MiApp.Services.Service.GestorDocumental.EliminarDocumento.Physical.StorageDeleteCompensationService>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.EliminarDocumento.Physical.IStorageDeleteAuditService, MiApp.Services.Service.GestorDocumental.EliminarDocumento.Physical.StorageDeleteAuditService>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.IWorkflowDeleteRelationResolver, MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.WorkflowDeleteRelationResolver>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.IStorageDeleteRestrictionDataProvider, MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.StorageDeleteRestrictionDataProvider>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.Policy.IDeleteOverridePolicy, MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.Policy.DeleteOverridePolicy>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.IStorageDeleteRestrictionPipeline, MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.StorageDeleteRestrictionPipeline>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.IStorageDeleteRestriction, MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.Rules.DeleteDocumentExistsRule>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.IStorageDeleteRestriction, MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.Rules.DeleteAlreadyRemovedRule>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.IStorageDeleteRestriction, MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.Rules.DeleteSharedDocumentRule>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.IStorageDeleteRestriction, MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.Rules.DeleteProductionRadicadoRule>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.IStorageDeleteRestriction, MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.Rules.DeleteOwnershipRule>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.IStorageDeleteRestriction, MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.Rules.DeleteDocuarchiGabinetePermissionRule>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.IStorageDeleteRestriction, MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.Rules.DeleteWorkflowStateRule>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.IStorageDeleteRestriction, MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.Rules.DeleteRadicadoInventarioRule>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.IStorageDeleteRestriction, MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.Rules.DeleteExpedienteStateRule>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.IStorageDeleteRestriction, MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.Rules.DeleteSignedDocumentRule>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.IStorageDeleteRestriction, MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.Rules.DeleteSignedVersionsRule>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.IStorageDeleteRestriction, MiApp.Services.Service.GestorDocumental.EliminarDocumento.Restriction.Rules.DeleteSnapshotConsistencyRule>();

            return services;
        }

        private static IServiceCollection AddStorageServices(this IServiceCollection services)
        {
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.IAlmacenarDocumentoUseCase, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.AlmacenarDocumentoUseCase>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.IDocumentStorageOrchestrator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.DocumentStorageOrchestrator>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Transaction.IStorageTransactionCoordinator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Transaction.StorageTransactionCoordinator>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.IStorageValidationPipeline, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.StorageValidationPipeline>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.IStorageAnexoRespuestaValidator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.StorageAnexoRespuestaValidator>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Errors.IStorageAnexoRespuestaErrorFactory, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Errors.StorageAnexoRespuestaErrorFactory>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.AnexosRespuesta.IStorageAnexoRespuestaMutationReportBuilder, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.AnexosRespuesta.StorageAnexoRespuestaMutationReportBuilder>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.IStorageValidator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.RequestStructureValidator>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.IStorageValidator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.DocumentoValidator>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.IStorageValidator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.CamposValidator>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.IStorageValidator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.TipoAlmacenamientoValidator>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.IStorageValidator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.ReglasBasicasValidator>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.IStorageValidator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.PreindexValidator>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.IStorageValidator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.GabineteRequiredFieldsValidator>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.IStorageValidator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.StorageOptionsValidator>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.IStorageValidator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.TrdRulesValidator>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.IStorageValidator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.ExpedienteUnidadRulesValidator>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Preindex.IStoragePreindexResolver, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Preindex.StoragePreindexResolver>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Preindex.IStoragePreindexReader, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Preindex.StoragePreindexReader>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Preindex.IStoragePreindexIntegrator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Preindex.StoragePreindexIntegrator>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Metadata.IStorageGabineteMetadataProvider, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Metadata.StorageGabineteMetadataProvider>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Metadata.IStorageDocumentMetadataAnalyzer, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Metadata.StorageDocumentMetadataAnalyzer>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Metadata.IStoragePageCountReader, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Metadata.StoragePageCountReader>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Metadata.IStorageSizeFormatter, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Metadata.StorageSizeFormatter>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Options.IStorageOptionsResolver, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Options.StorageOptionsResolver>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Identity.IStorageIdentityReservationCoordinator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Identity.StorageIdentityReservationCoordinator>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Identity.IStorageIdentityAllocator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Identity.StorageIdentityAllocator>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Identity.IStorageIdentityPolicy, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Identity.StorageIdentityPolicy>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Identity.IStorageDiskQuotaPolicy, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Identity.StorageDiskQuotaPolicy>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Expediente.IIndiceElectronicoCalculator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Expediente.IndiceElectronicoCalculator>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Expediente.IIndiceElectronicoBuilder, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Expediente.IndiceElectronicoBuilder>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Expediente.IExpedienteUnidadLegacyBuilder, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Expediente.ExpedienteUnidadLegacyBuilder>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Expediente.IExpedienteUnidadLegacyService, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Expediente.ExpedienteUnidadLegacyService>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.ExpedienteXml.IExpedienteIndiceXmlBuilder, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.ExpedienteXml.ExpedienteIndiceXmlBuilder>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.ExpedienteXml.IExpedienteIndiceXmlWriter, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.ExpedienteXml.ExpedienteIndiceXmlWriter>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.ExpedienteXml.IExpedienteIndiceXmlService, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.ExpedienteXml.ExpedienteIndiceXmlService>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Inventario.IInventarioDocumentalBuilder, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Inventario.InventarioDocumentalBuilder>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Builders.IStoragePlanBuilder, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Builders.StoragePlanBuilder>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Builders.IStorageXmlBuilder, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Builders.StorageXmlBuilder>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Physical.IStoragePhysicalPathService, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Physical.StoragePhysicalPathService>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Physical.IStorageFolderLegacyPolicy, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Physical.StorageFolderLegacyPolicy>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Physical.IStoragePathResolver, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Physical.StoragePathResolver>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Physical.IStorageFileWriter, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Physical.StorageFileWriter>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Physical.IStorageXmlWriter, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Physical.StorageXmlWriter>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Physical.IStorageCompensationManager, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Physical.StorageCompensationManager>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Compensation.IStorageDbCompensationService, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Compensation.StorageDbCompensationService>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Physical.IStoragePhysicalPhaseExecutor, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Physical.StoragePhysicalPhaseExecutor>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Workflow.IWorkflowStorageLogBuilder, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Workflow.WorkflowStorageLogBuilder>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Workflow.IWorkflowStorageLogService, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Workflow.WorkflowStorageLogService>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Descriptors.IStorageDescriptorResolver, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Descriptors.StorageDescriptorResolver>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Naming.IStorageExtensionResolver, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Naming.StorageExtensionResolver>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Naming.IStorageNamingService, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Naming.StorageNamingService>();

            return services;
        }

        private static IServiceCollection AddStorageRepositories(this IServiceCollection services)
        {
            services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Gabinete.IGabineteStorageRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Gabinete.GabineteStorageRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Inventario.IInventarioDocumentalRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Inventario.InventarioDocumentalRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Expediente.IExpedienteRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Expediente.ExpedienteRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Expediente.IExpedienteLegacyRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Expediente.ExpedienteLegacyRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Expediente.IUnidadConservacionLegacyRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Expediente.UnidadConservacionLegacyRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Expediente.IClaseDocumentoLegacyRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Expediente.ClaseDocumentoLegacyRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.ExpedienteXml.IExpedienteIndiceXmlRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.ExpedienteXml.ExpedienteIndiceXmlRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.UnidadConservacion.IUnidadConservacionRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.UnidadConservacion.UnidadConservacionRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.IndiceElectronico.IIndiceElectronicoRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.IndiceElectronico.IndiceElectronicoRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Workflow.IWorkflowStorageLogRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Workflow.WorkflowStorageLogRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.Common.Audit.ILogDocuarchiRepository, MiApp.Repository.Repositorio.GestorDocumental.Common.Audit.LogDocuarchiRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Compensation.IStorageDbCompensationRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Compensation.StorageDbCompensationRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.SystemStorage.ISystemStorageRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.SystemStorage.SystemStorageRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.SystemOptions.IStorageSystemOptionsRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.SystemOptions.StorageSystemOptionsRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.StorageRoute.IStorageRouteRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.StorageRoute.StorageRouteRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Disk.IStorageDiskQuotaRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Disk.StorageDiskQuotaRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.GabineteMetadata.IStorageGabineteMetadataRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.GabineteMetadata.StorageGabineteMetadataRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.CabinetIndexProviders.Radicacion.IRadicacionExpedienteValueRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.CabinetIndexProviders.Radicacion.RadicacionExpedienteValueRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.CabinetIndexProviders.Radicacion.IRadicacionDocumentoOrigenIndexRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.CabinetIndexProviders.Radicacion.RadicacionDocumentoOrigenIndexRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.CabinetIndexProviders.Radicacion.IRadicacionLegacySourceRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.CabinetIndexProviders.Radicacion.RadicacionLegacySourceRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestionCorrespondencia.GestionRespuesta.IRespuestaRadicadoAnexoWriteRepository, MiApp.Repository.Repositorio.GestionCorrespondencia.GestionRespuesta.RespuestaRadicadoAnexoWriteRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Descriptors.IStorageDescriptorCatalogRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Descriptors.StorageDescriptorCatalogRepository>();
            services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Extension.IStorageExtensionRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Extension.StorageExtensionRepository>();

            return services;
        }

        private static IServiceCollection AddStorageTemporaryUpload(this IServiceCollection services)
        {
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload.IStorageUploadPathResolver, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload.StorageUploadPathResolver>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload.IStorageUploadSessionStore, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload.StorageUploadSessionStore>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload.IStorageUploadPolicy, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload.StorageUploadPolicy>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload.IStorageLargeUploadService, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload.StorageLargeUploadService>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload.IStorageUploadCleanupService, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload.StorageUploadCleanupService>();

            return services;
        }

        private static IServiceCollection AddStorageCabinetIndex(this IServiceCollection services)
        {
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.CabinetIndexProviders.ICabinetIndexSeedEnvelopeValidationService, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.CabinetIndexProviders.CabinetIndexSeedEnvelopeValidationService>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.CabinetIndexProviders.ICabinetIndexProviderExecutionContextBuilder, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.CabinetIndexProviders.CabinetIndexProviderExecutionContextBuilder>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.CabinetIndexProviders.IStorageCabinetIndexProviderPolicyCatalog, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.CabinetIndexProviders.StorageCabinetIndexProviderPolicyCatalog>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.CabinetIndexProviders.IStorageCabinetIndexPolicyResolver, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.CabinetIndexProviders.StorageCabinetIndexPolicyResolver>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.CabinetIndexProviders.IStorageCabinetIndexProviderResolver, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.CabinetIndexProviders.StorageCabinetIndexProviderResolver>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.CabinetIndexProviders.ICabinetIndexFieldOwnershipPolicy, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.CabinetIndexProviders.CabinetIndexFieldOwnershipPolicy>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.CabinetIndexProviders.ICabinetIndexEffectiveFieldsService, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.CabinetIndexProviders.CabinetIndexEffectiveFieldsService>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.CabinetIndexProviders.ICabinetIndexValidationService, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.CabinetIndexProviders.CabinetIndexValidationService>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.CabinetIndexProviders.ICabinetIndexResolutionPoint, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.CabinetIndexProviders.CabinetIndexResolutionPoint>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.CabinetIndexProviders.Radicacion.IRadicacionCabinetIndexSourceService, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.CabinetIndexProviders.Radicacion.RadicacionCabinetIndexSourceService>();
            services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.CabinetIndexProviders.IStorageCabinetIndexProvider, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.CabinetIndexProviders.Radicacion.RadicacionCabinetIndexProvider>();

            return services;
        }

        private static IServiceCollection AddStorageHostedServices(this IServiceCollection services)
        {
            services.AddHostedService<DocuArchi.Api.Infrastructure.HostedServices.StorageUploadCleanupHostedService>();

            return services;
        }
    }
}
