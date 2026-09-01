using MiApp.Repository.Repositorio.Workflow.Terminar;
using MiApp.Services.Service.Workflow.Terminar;

namespace DocuArchi.Api.Infrastructure.DependencyInjection;

public static class WorkflowTerminarServiceCollectionExtensions
{
    public static IServiceCollection AddWorkflowTerminar(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITransitionConnectorRepository, TransitionConnectorRepository>();
        services.AddScoped<ITransitionConnectorResolver, TransitionConnectorResolver>();
        services.AddScoped<ITransitionRuleValidator, TransitionRuleValidator>();
        services.AddScoped<IWorkflowTerminationPreparationResolver, UnavailableWorkflowTerminationPreparationResolver>();
        services.AddScoped<IWorkflowTopologyMetadataReader, MySqlWorkflowTopologyMetadataReader>();
        services.AddScoped<IWorkflowAtomicTopologyGate, WorkflowAtomicTopologyGate>();
        services.AddScoped<IWorkflowMySqlTransactionSessionFactory, MySqlWorkflowTransactionSessionFactory>();
        services.AddScoped<IWorkflowTransitionUnitOfWork, MySqlWorkflowTransitionUnitOfWork>();
        services.AddScoped<IWorkflowIdempotencyStore, MySqlWorkflowIdempotencyStore>();
        services.AddScoped<IWorkflowIdempotencyRepository, WorkflowIdempotencyRepository>();
        services.AddScoped<IWorkflowTaskTransitionStore, MySqlWorkflowTaskTransitionStore>();
        services.AddScoped<IWorkflowTaskTransitionRepository, WorkflowTaskTransitionRepository>();
        services.AddScoped<IWorkflowSuccessorTaskStore, MySqlWorkflowSuccessorTaskStore>();
        services.AddScoped<IWorkflowSuccessorTaskRepository, WorkflowSuccessorTaskRepository>();
        services.AddScoped<IWorkflowRadicacionTransitionStore, MySqlWorkflowRadicacionTransitionStore>();
        services.AddScoped<IWorkflowRadicacionTransitionRepository, WorkflowRadicacionTransitionRepository>();
        services.AddScoped<IWorkflowTerminationAuditStore, MySqlWorkflowTerminationAuditStore>();
        services.AddScoped<IWorkflowTerminationAuditRepository, WorkflowTerminationAuditRepository>();
        services.AddScoped<ICompleteWorkflowTaskService, CompleteWorkflowTaskService>();
        return services;
    }
}
