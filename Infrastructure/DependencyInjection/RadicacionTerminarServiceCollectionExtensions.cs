using MiApp.Models.Models.Radicacion.Terminar;
using MiApp.Repository.Repositorio.Radicacion.Terminar;
using MiApp.Services.Service.Radicacion.Terminar;

namespace DocuArchi.Api.Infrastructure.DependencyInjection;

/// <summary>
/// Registra la consulta de politica de terminacion de radicacion.
/// Workflow se mantiene detras de un adaptador sin efectos hasta que SCRUM-374 integre su contrato estable.
/// </summary>
public static class RadicacionTerminarServiceCollectionExtensions
{
    public static IServiceCollection AddRadicacionTerminar(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IRadicationCompletionPolicyRepository, RadicationCompletionPolicyRepository>();
        services.AddScoped<IWorkflowTerminationPolicyGateway, UnavailableWorkflowTerminationPolicyGateway>();
        services.AddScoped<IRadicationCompletionPolicyService, RadicationCompletionPolicyService>();

        return services;
    }
}
