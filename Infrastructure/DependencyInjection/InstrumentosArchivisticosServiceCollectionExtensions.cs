using MiApp.Repository.Repositorio.GestorDocumental.InstrumentosArchivisticos;
using MiApp.Services.Service.GestorDocumental.InstrumentosArchivisticos;
namespace Microsoft.Extensions.DependencyInjection;
public static class InstrumentosArchivisticosServiceCollectionExtensions
{
    public static IServiceCollection AddInstrumentosArchivisticos(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IInstrumentoArchivisticoRepository, InstrumentoArchivisticoRepository>();
        services.AddScoped<IInstrumentoArchivisticoScopePolicy, InstrumentoArchivisticoScopePolicy>();
        services.AddScoped<IInstrumentoArchivisticoQueryService, InstrumentoArchivisticoQueryService>();
        return services;
    }
}
