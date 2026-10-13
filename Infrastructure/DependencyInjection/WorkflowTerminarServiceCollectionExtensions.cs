using MiApp.Repository.DataAccess;
using MiApp.Repository.Repositorio.Workflow.Terminar;
using MiApp.Services.Service.Workflow.Terminar;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DocuArchi.Api.Infrastructure.DependencyInjection;

public static class WorkflowTerminarServiceCollectionExtensions
{
    public static IServiceCollection AddWorkflowTerminar(this IServiceCollection services, IConfiguration configuration)
    {
        var engine = configuration["DbEngine"]?.Trim().ToLowerInvariant();
        if (engine is not ("mysql" or "sqlserver"))
        {
            throw new InvalidOperationException(
                "Workflow termination requires an explicitly supported DbEngine.");
        }

        services.AddScoped<ITransitionConnectorRepository, TransitionConnectorRepository>();
        services.AddScoped<IWorkflowActivityMetadataRepository, WorkflowActivityMetadataRepository>();
        services.AddScoped<IWorkflowSuccessorDestinationRepository, WorkflowSuccessorDestinationRepository>();
        services.AddScoped<IWorkflowVirtualDestinationRepository, WorkflowVirtualDestinationRepository>();
        services.AddScoped<IWorkflowTerminationPreparationTaskRepository, WorkflowTerminationPreparationTaskRepository>();
        services.AddScoped<ITransitionConnectorResolver, TransitionConnectorResolver>();
        services.AddScoped<IWorkflowActivityDestinationPolicyResolver, WorkflowActivityDestinationPolicyResolver>();
        services.AddScoped<IWorkflowSuccessorDestinationResolver, WorkflowSuccessorDestinationResolver>();
        services.AddScoped<ITransitionRuleValidator, TransitionRuleValidator>();
        services.AddScoped<IWorkflowDocumentRequirementEvaluator, WorkflowDocumentRequirementEvaluator>();
        services.AddScoped<IWorkflowRadicacionPreparationStore, WorkflowRadicacionPreparationStore>();
        services.AddScoped<IWorkflowRadicacionPreparationRepository, WorkflowRadicacionPreparationRepository>();
        services.AddScoped<IWorkflowManagedDestinationRepository, WorkflowManagedDestinationRepository>();
        services.AddScoped<IWorkflowManagedResponseEvaluator, WorkflowManagedResponseEvaluator>();
        services.AddScoped<IWorkflowManagedDestinationFingerprint, WorkflowManagedDestinationFingerprint>();
        services.AddScoped<IWorkflowManagedDestinationResolver, WorkflowManagedDestinationResolver>();
        services.AddDataProtection();
        services.AddOptions<ManagedDestinationPreviewTokenOptions>()
            .Bind(configuration.GetSection(ManagedDestinationPreviewTokenOptions.SectionName))
            .Validate(value => value.SchemaVersion > 0, "SchemaVersion must be positive.")
            .Validate(value => !string.IsNullOrWhiteSpace(value.Purpose), "Purpose is required.")
            .Validate(value => value.TimeToLive >= TimeSpan.FromSeconds(30)
                && value.TimeToLive <= TimeSpan.FromMinutes(15),
                "TimeToLive must be between 30 seconds and 15 minutes.")
            .Validate(value => value.SharedKeyRingConfirmed,
                "A persistent shared Data Protection key ring must be explicitly confirmed.")
            .ValidateOnStart();
        services.AddScoped<IManagedDestinationPreviewTokenService, ManagedDestinationPreviewTokenService>();
        services.AddScoped<IWorkflowTerminationPreparationResolver, WorkflowTerminationPreparationResolver>();
        if (engine == "sqlserver")
            services.AddScoped<IWorkflowTopologyMetadataReader, SqlServerWorkflowTopologyMetadataReader>();
        else
            services.AddScoped<IWorkflowTopologyMetadataReader, MySqlWorkflowTopologyMetadataReader>();
        if (engine == "sqlserver")
            services.AddScoped<IWorkflowDatabaseDialect, SqlServerWorkflowDatabaseDialect>();
        else
            services.AddScoped<IWorkflowDatabaseDialect, MySqlWorkflowDatabaseDialect>();
        services.AddScoped<IWorkflowPreviewIdentityRepository, WorkflowPreviewIdentityRepository>();
        services.AddScoped<IWorkflowTaskPreviewRepository, WorkflowTaskPreviewRepository>();
        services.AddScoped<IWorkflowConnectorPreviewRepository, WorkflowConnectorPreviewRepository>();
        services.AddScoped<IWorkflowPreviewAuthorizationService, WorkflowPreviewAuthorizationService>();
        services.AddScoped<IConsultarTransicionesDisponiblesService, ConsultarTransicionesDisponiblesService>();
        services.AddScoped<IWorkflowAtomicTopologyGate, WorkflowAtomicTopologyGate>();
        services.TryAddScoped<IDapperCrudTransactionalEngine>(provider =>
            provider.GetRequiredService<IDapperCrudEngine>() as IDapperCrudTransactionalEngine
            ?? throw new InvalidOperationException("The configured DapperCrudEngine does not support the transactional V2 contract."));
        services.AddScoped<IWorkflowMySqlTransactionSessionFactory, MySqlWorkflowTransactionSessionFactory>();
        services.AddScoped<IWorkflowSqlServerTransactionSessionFactory, SqlServerWorkflowTransactionSessionFactory>();
        if (engine == "sqlserver")
        {
            services.AddScoped<IWorkflowTransitionUnitOfWork, SqlServerWorkflowTransitionUnitOfWork>();
        }
        else
        {
            services.AddScoped<IWorkflowTransitionUnitOfWork, MySqlWorkflowTransitionUnitOfWork>();
        }
        if (engine == "sqlserver")
            services.AddScoped<IWorkflowIdempotencyLockReader, SqlServerWorkflowIdempotencyLockReader>();
        else
            services.AddScoped<IWorkflowIdempotencyLockReader, MySqlWorkflowIdempotencyLockReader>();
        services.AddScoped<IWorkflowIdempotencyStore, DapperCrudWorkflowIdempotencyStore>();
        services.AddScoped<IWorkflowIdempotencyRepository, WorkflowIdempotencyRepository>();
        if (engine == "sqlserver")
            services.AddScoped<IWorkflowTaskLockReader, SqlServerWorkflowTaskLockReader>();
        else
            services.AddScoped<IWorkflowTaskLockReader, MySqlWorkflowTaskLockReader>();
        services.AddScoped<IWorkflowTaskTransitionStore, DapperCrudWorkflowTaskTransitionStore>();
        services.AddScoped<IWorkflowTaskTransitionRepository, WorkflowTaskTransitionRepository>();
        services.AddScoped<IWorkflowSuccessorTaskStore, DapperCrudWorkflowSuccessorTaskStore>();
        services.AddScoped<IWorkflowStateStore>(provider =>
            provider.GetRequiredService<IWorkflowSuccessorTaskStore>());
        services.AddScoped<IWorkflowSuccessorTaskRepository, WorkflowSuccessorTaskRepository>();
        services.AddScoped<IWorkflowStateRepository>(provider =>
            provider.GetRequiredService<IWorkflowSuccessorTaskRepository>());
        services.AddScoped<IWorkflowRadicacionTransitionStore, DapperCrudWorkflowRadicacionTransitionStore>();
        services.AddScoped<IWorkflowRadicacionTransitionRepository, WorkflowRadicacionTransitionRepository>();
        services.AddScoped<IWorkflowTerminationAuditStore, DapperCrudWorkflowTerminationAuditStore>();
        services.AddScoped<IWorkflowTerminationAuditRepository, WorkflowTerminationAuditRepository>();
        services.AddScoped<IWorkflowEventCatalogRepository, DisabledWorkflowEventCatalogRepository>();
        services.AddScoped<IWorkflowOutboxRepository, DisabledWorkflowOutboxRepository>();
        services.AddScoped<IWorkflowEventCatalog, WorkflowEventCatalog>();
        services.AddScoped<IWorkflowEventHandlerRegistry, WorkflowEventHandlerRegistry>();
        services.AddScoped<IWorkflowBeforeTransitionPipeline, WorkflowBeforeTransitionPipeline>();
        services.AddScoped<IWorkflowOutboxIntentFactory, WorkflowOutboxIntentFactory>();
        services.AddScoped<IWorkflowOutboxDispatcher, WorkflowOutboxDispatcher>();
        services.AddScoped<IWorkflowTaskNotificationContextRepository, WorkflowTaskNotificationContextRepository>();
        services.AddScoped<IWorkflowTaskNotificationRecipientRepository, WorkflowTaskNotificationRecipientRepository>();
        services.AddScoped<IWorkflowTaskNotificationMessageComposer, WorkflowTaskNotificationMessageComposer>();
        services.AddScoped<IWorkflowTaskEmailDispatchRequestFactory, WorkflowTaskEmailDispatchRequestFactory>();
        if (configuration.GetValue<bool>("EmailDispatch:EnableDispatchService"))
        {
            services.AddScoped<IWorkflowTaskNotificationDispatcher, WorkflowTaskNotificationDispatcher>();
        }
        else
        {
            services.AddScoped<IWorkflowTaskNotificationDispatcher, DisabledWorkflowTaskNotificationDispatcher>();
        }
        services.AddScoped<ICompleteWorkflowTaskService, CompleteWorkflowTaskService>();
        return services;
    }
}
