using MiApp.Repository.Repositorio.GestorDocumental.TipologiasDocumentales;
using MiApp.Services.Service.GestorDocumental.TipologiasDocumentales;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DocuArchi.Api.Infrastructure.DependencyInjection
{
    public static class GestorDocumentalServiceCollectionExtensions
    {
        public static IServiceCollection AddGestorDocumental(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddTipologiasDocumentales();

            return services;
        }

        private static IServiceCollection AddTipologiasDocumentales(this IServiceCollection services)
        {
            services.AddScoped<IServiceSolicitaListaTipologiasDocumentales, ServiceSolicitaListaTipologiasDocumentales>();
            services.AddScoped<ISolicitaListaTipologiasDocumentalesRepository, SolicitaListaTipologiasDocumentalesRepository>();
            services.AddScoped<IContextoTipologiaResolverService, ContextoTipologiaResolverService>();
            services.AddScoped<IContextoTipologiaResolverRepository, SolicitaListaTipologiasDocumentalesRepository>();

            return services;
        }
    }
}
