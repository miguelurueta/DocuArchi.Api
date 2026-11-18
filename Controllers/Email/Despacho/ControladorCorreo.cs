using System.Diagnostics;
using MiApp.DTOs.DTOs.Email.Core;
using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.Email;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DocuArchi.Api.Controllers.Email.Despacho;

[ApiController]
[Authorize]
[Route("api/correo")]
public sealed class ControladorCorreo(
    IServiceDespachoCorreo service,
    IClaimValidationService claimValidation,
    ILogger<ControladorCorreo> logger) : ControllerBase
{
    private static readonly ActivitySource ActivitySource = new("MiApp.Email.Controller");

    [HttpPost("enviar")]
    [RequestSizeLimit(512_000)]
    [ProducesResponseType(typeof(AppResponses<RespuestaEnvioCorreoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppResponses<RespuestaEnvioCorreoDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AppResponses<RespuestaEnvioCorreoDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(AppResponses<RespuestaEnvioCorreoDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(AppResponses<RespuestaEnvioCorreoDto>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(AppResponses<RespuestaEnvioCorreoDto>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(AppResponses<RespuestaEnvioCorreoDto>), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<AppResponses<RespuestaEnvioCorreoDto>>> EnviarAsync(
        [FromBody] SolicitudEnvioCorreoDto? request,
        CancellationToken ct)
    {
        using var fallbackActivity = Activity.Current is null
            ? new Activity("EmailController.RequestDispatch").SetIdFormat(ActivityIdFormat.W3C).Start() : null;
        var started = Stopwatch.GetTimestamp();
        string errorCode = "";
        var metaStatus = "ERROR";
        int? usuarioId = int.TryParse(User.FindFirst("usuarioid")?.Value ?? User.FindFirst("uid")?.Value, out var resolvedUser) && resolvedUser > 0 ? resolvedUser : null;
        var requestId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
        var externalRequestId = Request.Headers.TryGetValue("x-request-id", out var header)
            ? header.ToString().Trim() : requestId;
        using var activity = ActivitySource.StartActivity("EmailController.RequestDispatch");
        activity?.SetTag("request_id", requestId);
        activity?.SetTag("x_request_id", externalRequestId);
        activity?.SetTag("correlation_id", request?.CorrelationId);
        AppResponses<RespuestaEnvioCorreoDto> Reject(string code, string message, string? field = null)
        { errorCode = code; return Error(code, message, requestId, field); }
        try
        {
            if (request is null)
                return BadRequest(Reject("EMAIL_REQUEST_INVALID", "La solicitud de correo es obligatoria.", "request"));
            if (ContainsHeaderInjection(request))
                return BadRequest(Reject("EMAIL_CRLF_INVALID", "La solicitud contiene caracteres no permitidos."));
            var alias = claimValidation.ValidateClaim<string>("defaulalias");
            if (!alias.Success || string.IsNullOrWhiteSpace(alias.ClaimValue))
                return Unauthorized(Reject("EMAIL_ALIAS_REQUIRED", "No fue posible validar el contexto de la solicitud.", "defaulalias"));
            var empresa = claimValidation.ValidateClaim<string>("emp");
            if (!empresa.Success || !int.TryParse(empresa.ClaimValue, out var idEmpresa) || idEmpresa <= 0)
                return Unauthorized(Reject("EMAIL_COMPANY_REQUIRED", "No fue posible validar la empresa de la solicitud.", "emp"));
            ct.ThrowIfCancellationRequested();
            var result = await service.SolicitarEnvioAsync(request, alias.ClaimValue.Trim(), idEmpresa, ct).ConfigureAwait(false);
            result.meta ??= new AppMeta();
            result.meta.RequestId ??= requestId;
            result.meta.Metadata ??= new Dictionary<string, object?>();
            result.meta.Metadata["xRequestId"] = externalRequestId;
            activity?.SetTag("metaStatus", result.meta.Status ?? (result.success ? "OK" : "ERROR"));
            metaStatus = result.success ? "OK" : "ERROR";
            errorCode = result.errors?.OfType<AppError>().FirstOrDefault()?.Code ?? "";
            if (result.success) return Ok(result);
            return StatusCode(MapStatus(result), result);
        }
        catch (OperationCanceledException) { activity?.SetStatus(ActivityStatusCode.Error, "EMAIL_CANCELLED"); throw; }
        catch (Exception ex)
        {
            errorCode = "EMAIL_CONTROLLER_ERROR";
            logger.LogError("Email controller failure requestId={RequestId} stage=controller operation=EnviarAsync errorCode={ErrorCode}", requestId, errorCode);
            activity?.SetStatus(ActivityStatusCode.Error, "EMAIL_CONTROLLER_ERROR");
            return StatusCode(StatusCodes.Status500InternalServerError, Error("EMAIL_CONTROLLER_ERROR", "No fue posible procesar la solicitud de correo.", requestId, cause: ex));
        }
        finally
        {
            logger.LogInformation("Email HTTP requestId={RequestId} stage=controller operation=EnviarAsync errorCode={ErrorCode} metaStatus={MetaStatus} durationMs={DurationMs} usuarioId={UsuarioId}", requestId, errorCode, metaStatus, Stopwatch.GetElapsedTime(started).TotalMilliseconds, usuarioId);
        }
    }

    private static bool ContainsHeaderInjection(SolicitudEnvioCorreoDto request) =>
        ContainsCrLf(request.CodigoProceso) || ContainsCrLf(request.EntidadNegocio) || ContainsCrLf(request.IdEntidadNegocio) ||
        ContainsCrLf(request.CorrelationId) || ContainsCrLf(request.IdempotencyKey) || ContainsCrLf(request.FromEmail) ||
        ContainsCrLf(request.Subject) || ContainsCrLf(request.NumeroRadicado) || ContainsCrLf(request.ReferenciaFuncional);
    private static bool ContainsCrLf(string? value) => value?.Contains('\r') == true || value?.Contains('\n') == true;

    private static int MapStatus(AppResponses<RespuestaEnvioCorreoDto> result)
    {
        var code = result.errors?.OfType<AppError>().Select(e => e.Code).FirstOrDefault(c => !string.IsNullOrWhiteSpace(c)) ?? string.Empty;
        if (code is "EMAIL_SENDER_NOT_CONFIGURED" or "EMAIL_SENDER_CONFIGURATION_INVALID") return StatusCodes.Status409Conflict;
        if (code.Contains("IDEMPOTENCY_CONFLICT", StringComparison.OrdinalIgnoreCase)) return StatusCodes.Status409Conflict;
        if (code == "EMAIL_ATTACHMENT_ACCESS_DENIED") return StatusCodes.Status403Forbidden;
        if (code is "EMAIL_ATTACHMENTS_DISABLED" or "EMAIL_ATTACHMENT_PROVIDER_UNAVAILABLE" or "EMAIL_ATTACHMENT_SOURCE_UNAVAILABLE"
            or "EMAIL_ATTACHMENT_SCANNER_UNAVAILABLE" or "EMAIL_ATTACHMENT_STORAGE_UNAVAILABLE")
            return StatusCodes.Status503ServiceUnavailable;
        if (code is "EMAIL_ATTACHMENT_LIMIT_EXCEEDED" or "EMAIL_ATTACHMENT_CONTENT_REJECTED" or "EMAIL_ATTACHMENT_MISSING")
            return StatusCodes.Status400BadRequest;
        if (code.Contains("QUEUE", StringComparison.OrdinalIgnoreCase) || code.Contains("REPOSITORY", StringComparison.OrdinalIgnoreCase)) return StatusCodes.Status503ServiceUnavailable;
        if (code is "EMAIL_RECIPIENT_VALIDATION_UNAVAILABLE") return StatusCodes.Status503ServiceUnavailable;
        if (code.Contains("INVALID", StringComparison.OrdinalIgnoreCase) || code.Contains("REQUIRED", StringComparison.OrdinalIgnoreCase) || code is "EMAIL_RECIPIENT_NOT_FOUND") return StatusCodes.Status400BadRequest;
        return StatusCodes.Status500InternalServerError;
    }

    private static AppResponses<RespuestaEnvioCorreoDto> Error(string code, string userMessage, string requestId, string? field = null, Exception? cause = null) => new()
    {
        success = false, message = userMessage, data = default!,
        meta = new AppMeta { RequestId = requestId, Status = "ERROR", ResponseVersion = "1.0", Metadata = EmailFailureDiagnostics.CreateMetadata("controller", "EnviarAsync", cause) },
        errors = [new AppError { Type = "Validation", Code = code, UserMessage = userMessage, Message = userMessage, Field = field }]
    };
}
