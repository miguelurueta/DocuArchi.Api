using DocuArchi.Api.Controllers.GestorDocumental.TipologiasDocumentales.Actualizacion.WorkflowRadicacion;
using MiApp.Services.Service.GestorDocumental.TipologiasDocumentales.Actualizacion;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace DocuArchi.Api.Infrastructure.TipologiasDocumentales.Actualizacion;

/// <summary>Normaliza challenge/forbid exclusivamente en la acción nueva y delega intacto el comportamiento de otras rutas.</summary>
public sealed class ResultadoAutorizacionActualizacionTipologia : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _predeterminado = new();

    /// <summary>Emite 401/403 del catálogo propio sin alterar opciones ni eventos globales JWT.</summary>
    /// <param name="next">Pipeline posterior cuando la autorización lo permite.</param>
    /// <param name="context">Contexto de la petición y principal autenticado por el esquema existente.</param>
    /// <param name="policy">Política resuelta por el middleware.</param>
    /// <param name="authorizeResult">Resultado real de autorización; no se infiere del body.</param>
    /// <returns>Tarea con respuesta propia en la ruta nueva o delegación al handler estándar.</returns>
    /// <remarks>Nombre impuesto por IAuthorizationMiddlewareResultHandler.HandleAsync; excepción LEG-07.</remarks>
    public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        var accion = context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
        if (accion?.ControllerTypeInfo.AsType() != typeof(ActualizarTipologiaWorkflowRadicacionController) || authorizeResult.Succeeded)
        { await _predeterminado.HandleAsync(next, context, policy, authorizeResult).ConfigureAwait(false); return; }
        var fabrica = context.RequestServices.GetRequiredService<IErrorActualizacionTipologiaFactory>();
        var causa = context.Features.Get<IAuthenticateResultFeature>()?.AuthenticateResult?.Failure;
        var resultado = fabrica.CrearRespuestaError(authorizeResult.Challenged ? "TIPDOC-UPD-AUTH-REQUIRED" : "TIPDOC-UPD-ACCESS-DENIED",
            FronteraHttpActualizacionTipologia.ObtenerCorrelacionSegura(context), "auth", "AutorizarActualizacionTipologia", 0, causa: causa);
        context.Response.StatusCode = resultado.EstadoHttp;
        if (authorizeResult.Challenged) context.Response.Headers.WWWAuthenticate = "Bearer";
        await context.Response.WriteAsJsonAsync(resultado.Respuesta, FronteraHttpActualizacionTipologia.OpcionesJson, context.RequestAborted).ConfigureAwait(false);
    }
}
