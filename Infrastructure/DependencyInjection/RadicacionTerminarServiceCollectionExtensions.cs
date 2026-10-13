using MiApp.Models.Models.Radicacion.Terminar;
using MiApp.Repository.Repositorio.Radicacion.Terminar;
using MiApp.Services.Service.Radicacion.Terminar;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DocuArchi.Api.Infrastructure.DependencyInjection;

/// <summary>
/// Registra la consulta de politica de terminacion de radicacion.
/// Las modalidades Workflow usan un gateway de solo lectura y fallan cerradas cuando
/// la capacidad complementaria de destino gestionado no esta disponible.
/// </summary>
public static class RadicacionTerminarServiceCollectionExtensions
{
    public static IServiceCollection AddRadicacionTerminar(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IRadicationCompletionPolicyRepository, RadicationCompletionPolicyRepository>();
        services.TryAddScoped<IWorkflowManagedDestinationPolicyResolver,
            UnavailableWorkflowManagedDestinationPolicyResolver>();
        services.AddScoped<IWorkflowTerminationPolicyGateway, WorkflowTerminationPolicyGateway>();
        services.AddScoped<IRadicationCompletionPolicyService, RadicationCompletionPolicyService>();

        return services;
    }
}
