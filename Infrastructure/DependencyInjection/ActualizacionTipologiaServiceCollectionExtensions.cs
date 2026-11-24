using DocuArchi.Api.Infrastructure.TipologiasDocumentales.Actualizacion;
using MiApp.Repository.Repositorio.GestorDocumental.TipologiasDocumentales.Actualizacion;
using MiApp.Services.Service.GestorDocumental.TipologiasDocumentales.Actualizacion;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DocuArchi.Api.Infrastructure.DependencyInjection;

/// <summary>Registra solo el recorrido nuevo junto al módulo existente de tipologías.</summary>
public static class ActualizacionTipologiaServiceCollectionExtensions
{
    /// <summary>Agrega interfaces, frontera HTTP y servicio propio de recuperación sin reemplazar almacenamiento ni el GET.</summary>
    /// <param name="servicios">Contenedor existente de la API.</param>
    /// <returns>Misma colección con los componentes de actualización.</returns>
    public static IServiceCollection AgregarActualizacionTipologiaDocumental(this IServiceCollection servicios)
    {
        servicios.AddScoped<IDocumentoInventarioTipologiaRepository, DocumentoInventarioTipologiaRepository>();
        servicios.AddScoped<IContextoActualizacionTipologiaRepository, ContextoActualizacionTipologiaRepository>();
        servicios.AddScoped<IReglaActualizacionTipologiaRepository, ReglaActualizacionTipologiaRepository>();
        servicios.AddScoped<IPoliticaRetiradaTipologiaRepository, PoliticaRetiradaTipologiaRepository>();
        servicios.AddScoped<IRestriccionesFirmaTipologiaRepository, RestriccionesFirmaTipologiaRepository>();
        servicios.AddScoped<IExpedienteActualizacionTipologiaRepository, ExpedienteActualizacionTipologiaRepository>();
        servicios.AddScoped<IUnicidadActualizacionTipologiaRepository, UnicidadActualizacionTipologiaRepository>();
        servicios.AddScoped<IEscrituraActualizacionTipologiaRepository, EscrituraActualizacionTipologiaRepository>();
        servicios.AddScoped<IEvidenciaAuditoriaTipologiaRepository, EvidenciaAuditoriaTipologiaRepository>();
        servicios.AddScoped<IUbicacionesRecuperacionTipologiaRepository, UbicacionesRecuperacionTipologiaRepository>();
        servicios.AddScoped<IPlanificadorMutacionTipologiaDocumental, PlanificadorMutacionTipologiaDocumental>();
        servicios.AddScoped<IPreparadorXmlTipologiaDocumental, PreparadorXmlTipologiaDocumental>();
        servicios.AddScoped<IRegistroDurableXmlTipologia, RegistroDurableXmlTipologia>();
        servicios.AddScoped<IExploradorManifiestosTipologia, ExploradorManifiestosTipologia>();
        servicios.AddScoped<IAutorizacionActualizacionTipologiaService, AutorizacionActualizacionTipologiaService>();
        servicios.AddScoped<IPreparadorTransaccionalTipologia, PreparadorTransaccionalTipologia>();
        servicios.AddScoped<IUnidadSqlActualizacionTipologia, UnidadSqlActualizacionTipologia>();
        servicios.AddScoped<IRecuperadorActualizacionTipologia, RecuperadorActualizacionTipologia>();
        servicios.AddScoped<ICoordinadorActualizacionTipologia, CoordinadorActualizacionTipologia>();
        servicios.AddScoped<IErrorActualizacionTipologiaFactory, ErrorActualizacionTipologiaFactory>();
        servicios.AddScoped<IActualizacionTipologiaWorkflowRadicacionService, ActualizacionTipologiaWorkflowRadicacionService>();
        servicios.AddScoped<FronteraHttpActualizacionTipologia>();
        servicios.Replace(ServiceDescriptor.Singleton<IAuthorizationMiddlewareResultHandler, ResultadoAutorizacionActualizacionTipologia>());
        servicios.AddHostedService<ServicioInicioRecuperacionTipologia>();
        return servicios;
    }
}
