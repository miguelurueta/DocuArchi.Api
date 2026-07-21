using MiApp.Repository.Repositorio.Radicador.PlantillaValidacion;
using MiApp.Services.Service.Radicacion.PlantillaValidacion;

namespace DocuArchi.Api.Infrastructure.DependencyInjection
{
    public static class TercerosPlantillaValidacionServiceCollectionExtensions
    {
        public static IServiceCollection AddTercerosPlantillaValidacion(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<ITercerosPlantillaValidacionRepository, TercerosPlantillaValidacionRepository>();
            services.AddScoped<ITercerosPlantillaValidacionService, TercerosPlantillaValidacionService>();

            return services;
        }
    }
}
