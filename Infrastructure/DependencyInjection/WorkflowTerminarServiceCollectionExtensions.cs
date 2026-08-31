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
        return services;
    }
}
