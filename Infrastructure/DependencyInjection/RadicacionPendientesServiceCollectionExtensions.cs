using MiApp.Repository.Repositorio.Radicador.Tramite;
using MiApp.Services.Service.Radicacion.Tramite;

namespace DocuArchi.Api.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Registra la composicion del listado de pendientes de radicacion.
    /// </summary>
    public static class RadicacionPendientesServiceCollectionExtensions
    {
        public static IServiceCollection AddRadicacionPendientes(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<IListaRadicadosPendientesRepository, ListaRadicadosPendientesRepository>();
            services.AddScoped<IListaRadicadosPendientesService, ListaRadicadosPendientesService>();
            services.AddScoped<IRadicacionPendienteEstadoActivoRepository, RadicacionPendienteEstadoActivoRepository>();
            services.AddScoped<IRadicacionPendienteEstadoActivoService, RadicacionPendienteEstadoActivoService>();

            return services;
        }
    }
}
