using MiApp.Repository.Repositorio.GestorDocumental.InstrumentosArchivisticos;
using MiApp.Services.Service.GestorDocumental.InstrumentosArchivisticos;
namespace Microsoft.Extensions.DependencyInjection;
public static class InstrumentosArchivisticosServiceCollectionExtensions
{
    public static IServiceCollection AddInstrumentosArchivisticos(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IInstrumentoArchivisticoRepository, InstrumentoArchivisticoRepository>();
        services.AddScoped<IInstrumentoArchivisticoCicloRepository, InstrumentoArchivisticoCicloRepository>();
        services.AddScoped<IInstrumentoArchivisticoScopePolicy, InstrumentoArchivisticoScopePolicy>();
        services.AddScoped<IInstrumentoArchivisticoQueryService, InstrumentoArchivisticoQueryService>();
        services.AddScoped<IInstrumentoArchivisticoCicloService, InstrumentoArchivisticoCicloService>();
        return services;
    }
}
