using MiApp.Repository.Repositorio.GestorDocumental.InstrumentosArchivisticos;
using MiApp.Services.Service.GestorDocumental.InstrumentosArchivisticos;
using DocuArchi.Api.Infrastructure.Observability;
namespace Microsoft.Extensions.DependencyInjection;
public static class InstrumentosArchivisticosServiceCollectionExtensions
{
    public static IServiceCollection AddInstrumentosArchivisticos(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IInstrumentoArchivisticoRepository, InstrumentoArchivisticoRepository>();
        services.AddScoped<IInstrumentoArchivisticoCicloRepository, InstrumentoArchivisticoCicloRepository>();
        services.AddScoped<IInstrumentoArchivisticoRetencionRepository, InstrumentoArchivisticoRetencionRepository>();
        services.AddScoped<IInstrumentoArchivisticoScopePolicy, InstrumentoArchivisticoScopePolicy>();
        services.AddScoped<IInstrumentoArchivisticoQueryService, InstrumentoArchivisticoQueryService>();
        services.AddScoped<IInstrumentoArchivisticoCicloService, InstrumentoArchivisticoCicloService>();
        services.AddScoped<IInstrumentoArchivisticoRetencionService, InstrumentoArchivisticoRetencionService>();
        services.AddScoped<IRequestCorrelationContext, RequestCorrelationContext>();
        return services;
    }
}
