using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DocuArchi.Api.Infrastructure.Filters;
using MiApp.DTOs.DTOs.GestionCorrespondencia.GestionRespuesta.Core.Preparacion;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestionCorrespondencia.GestionRespuesta.Core.Preparacion;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using MiApp.Services.Service.Seguridad.Autorizacion.Permisos;

namespace DocuArchi.Api.Controllers.GestionCorrespondencia.GestionRespuesta.Core.Preparacion;

/// <summary>Expone la preparación sin escrituras con identidad resuelta exclusivamente desde el servidor.</summary>
/// <param name="service">Caso de uso que verifica responsabilidad y asignación antes de componer datos.</param>
/// <param name="currentUser">Claims y permisos del actor autenticado.</param>
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
    /// <param name="idRespuestaRadicado">Respuesta cuya responsabilidad se verificará.</param>
    /// <param name="idTareaWf">Tarea que debe pertenecer a la respuesta y al actor Workflow.</param>
    /// <param name="ct">Cancelación de la consulta.</param>
    /// <returns>Envelope de preparación o rechazo/error seguro con correlación y sin caché.</returns>
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
            currentUser.HasPermission(DocuArchiPermissionsRA.GESTION_RESPUESTA))
        { AliasWorkflow = currentUser.GetClaimValue("defaulaliaswf") ?? "" };
        var response = await service.Preparar(idRespuestaRadicado, idTareaWf, actor, requestId, ct);
        return StatusCode(PreparacionRespuestaErrorResponseFactory.HttpStatus(response), response);
    }
}

