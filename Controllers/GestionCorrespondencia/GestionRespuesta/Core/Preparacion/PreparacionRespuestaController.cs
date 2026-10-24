using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MiApp.DTOs.DTOs.GestionCorrespondencia.GestionRespuesta.Core.Preparacion;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestionCorrespondencia.GestionRespuesta.Core.Preparacion;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using MiApp.Services.Service.Seguridad.Autorizacion.Permisos;

namespace DocuArchi.Api.Controllers.GestionCorrespondencia.GestionRespuesta.Core.Preparacion;

[ApiController]
[Authorize]
[PreparacionModelState]
[Route("api/gestion-correspondencia/respuestas")]
public sealed class PreparacionRespuestaController(IPreparacionRespuestaService service, ICurrentUserService currentUser) : ControllerBase
{
    /// <summary>Consulta autorizada sin escrituras para preparar el formulario. No formaliza ni envía.</summary>
    /// <remarks>Puede devolver bloqueos funcionales con HTTP 200. La confirmación permanece deshabilitada.
    /// Correo principal de solo lectura; sin listado ni cantidad de adjuntos. No requiere Idempotency-Key.
    /// El servidor relee los datos; referenciaPreparacion no concede autorización ni acepta un snapshot cliente.</remarks>
    [HttpGet("{idRespuestaRadicado:long}/preparacion")]
    [ProducesResponseType(typeof(AppResponses<PreparacionRespuestaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppResponses<PreparacionRespuestaDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(AppResponses<PreparacionRespuestaDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(AppResponses<PreparacionRespuestaDto>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(AppResponses<PreparacionRespuestaDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AppResponses<PreparacionRespuestaDto?>>> Get(long idRespuestaRadicado,
        [FromQuery] long idTareaWf, CancellationToken ct)
    {
        Response.Headers.CacheControl = "no-store";
        var requestId = HttpContext.TraceIdentifier;
        Response.Headers["X-Request-Id"] = requestId;
        _ = int.TryParse(currentUser.GetClaimValue("emp"), out var empresaId);
        var actor = new ActorPreparacion(currentUser.UsuarioIdInt, empresaId, currentUser.Alias ?? "",
            currentUser.HasPermission(DocuArchiPermissionsRA.GESTION_RESPUESTA));
        var response = await service.Preparar(idRespuestaRadicado, idTareaWf, actor, requestId, ct);
        return StatusCode(PreparacionRespuestaErrorResponseFactory.HttpStatus(response), response);
    }
}

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
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<PreparacionRespuestaController>>();
        logger.LogInformation("stage=preparacion operation=BindRequest errorCode=PREPARACION_REQUEST_INVALID metaStatus=rejected durationMs=0 requestId={RequestId}", requestId);
    }
}
