using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.Radicacion.Terminar;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Models.Models.Radicacion.Terminar;
using MiApp.Services.Service.Radicacion.Terminar;
using MiApp.Services.Service.Radicacion.Terminar.Errors;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.ComponentModel.DataAnnotations;

namespace DocuArchi.Api.Controllers.Radicacion.Terminar;

[ApiController, Authorize, Route("api/radicacion/estados")]
public sealed class CloseRadicationController(ICloseRadicationService service, IClaimValidationService claims,
    ILogger<CloseRadicationController> logger) : ControllerBase
{
    [HttpPost("{idEstadoRadicado:long}/cierre"), CloseRadicationModelValidation]
    [ProducesResponseType(typeof(AppResponses<CloseRadicationResponseDto>), 200)]
    [ProducesResponseType(typeof(AppResponses<CloseRadicationResponseDto>), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(typeof(AppResponses<CloseRadicationResponseDto>), 403)]
    [ProducesResponseType(typeof(AppResponses<CloseRadicationResponseDto>), 404)]
    [ProducesResponseType(typeof(AppResponses<CloseRadicationResponseDto>), 409)]
    [ProducesResponseType(typeof(AppResponses<CloseRadicationResponseDto>), 500)]
    [ProducesResponseType(typeof(AppResponses<CloseRadicationResponseDto>), 503)]
    public async Task<ActionResult<AppResponses<CloseRadicationResponseDto>>> Close(long idEstadoRadicado,
        [FromBody] CloseRadicationRequestDto? request,
        [FromHeader(Name = "Idempotency-Key"), Required, StringLength(128)] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var started = System.Diagnostics.Stopwatch.StartNew();
        try
        {
        var identity = claims.ValidateClaim<string>("usuarioid");
        var alias = claims.ValidateClaim<string>("defaulalias");
        var userId = identity.Success && int.TryParse(identity.ClaimValue, out var value) ? value : 0;
        var keys = Request.Headers["Idempotency-Key"];
        var response = await service.CloseAsync(idEstadoRadicado, userId, alias.Success ? alias.ClaimValue ?? "" : "",
            keys.Count == 1 ? idempotencyKey : null, request, HttpContext.TraceIdentifier,
            HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        if (response.success) return Ok(response);
        var code = response.errors?.OfType<AppError>().FirstOrDefault()?.Code;
        var descriptor = Enum.GetValues<CloseRadicationFailureKind>().Select(CloseRadicationErrorCatalog.Get).FirstOrDefault(x => x.Code == code);
        return StatusCode(descriptor?.HttpStatus ?? 500, response);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            var requestId = CloseRadicationSanitizacion.RequestId(HttpContext.TraceIdentifier);
            var failure = ex as CloseRadicationException ?? new(CloseRadicationFailureKind.Unexpected,
                "operation", "UNEXPECTED_FAILURE", ex);
            var response = CloseRadicationErrorResponseFactory.Create<CloseRadicationResponseDto>(failure, requestId);
            logger.LogWarning("CloseRadication requestId={requestId} stage={stage} operation={operation} errorCode={errorCode} metaStatus={metaStatus} durationMs={durationMs}",
                requestId, response.meta!.Metadata!["Stage"], "CloseRadication", response.message, "error", started.ElapsedMilliseconds);
            return StatusCode(CloseRadicationErrorCatalog.Get(failure.Kind).HttpStatus, response);
        }
    }
}

// Run before ApiController's default model-state filter; never echo binding errors/body.
public sealed class CloseRadicationModelValidationAttribute : ActionFilterAttribute
{
    public CloseRadicationModelValidationAttribute() => Order = -3000;
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid) return;
        var requestId = CloseRadicationSanitizacion.RequestId(context.HttpContext.TraceIdentifier);
        var response = CloseRadicationErrorResponseFactory.Create<CloseRadicationResponseDto>(
            new(CloseRadicationFailureKind.InvalidRequest, "validation", "REQUEST_INVALID"), requestId);
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<CloseRadicationModelValidationAttribute>>();
        logger.LogWarning("CloseRadication requestId={requestId} stage={stage} operation={operation} errorCode={errorCode} metaStatus={metaStatus} durationMs={durationMs}",
            requestId, "validation", "CloseRadication", response.message, "error", 0);
        context.Result = new BadRequestObjectResult(response);
    }
}
