using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MiApp.Services.Service.GestionCorrespondencia.GestionRespuesta.Core.Preparacion;

namespace DocuArchi.Api.Infrastructure.Filters;

// Solo este contrato nuevo: conservar respuestas de model binding de otros controllers.
public sealed class PreparacionModelStateAttribute : ActionFilterAttribute
{
    public PreparacionModelStateAttribute() => Order = -3000;
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid) return;
        var requestId = context.HttpContext.TraceIdentifier;
        context.HttpContext.Response.Headers["X-Request-Id"] = requestId;
        context.HttpContext.Response.Headers.CacheControl = "no-store";
        var response = PreparacionRespuestaErrorResponseFactory.Failure("PREPARACION_REQUEST_INVALID", requestId);
        context.Result = new ObjectResult(response) { StatusCode = 400 };
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<PreparacionModelStateAttribute>>();
        logger.LogInformation("stage=preparacion operation=BindRequest errorCode=PREPARACION_REQUEST_INVALID metaStatus=rejected durationMs=0 requestId={RequestId}", requestId);
    }
}
