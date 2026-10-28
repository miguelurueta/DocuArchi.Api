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
    [ProducesResponseType(typeof(AppResponses<RespuestaEnvioCorreoDto>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(AppResponses<RespuestaEnvioCorreoDto>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(AppResponses<RespuestaEnvioCorreoDto>), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<AppResponses<RespuestaEnvioCorreoDto>>> EnviarAsync(
        [FromBody] SolicitudEnvioCorreoDto? request,
        CancellationToken ct)
    {
        var requestId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
        var externalRequestId = Request.Headers.TryGetValue("x-request-id", out var header)
            ? header.ToString().Trim() : requestId;
        using var activity = ActivitySource.StartActivity("EmailController.RequestDispatch");
        activity?.SetTag("request_id", requestId);
        activity?.SetTag("x_request_id", externalRequestId);
        activity?.SetTag("correlation_id", request?.CorrelationId);
        try
        {
            if (request is null)
                return BadRequest(Error("EMAIL_REQUEST_INVALID", "La solicitud de correo es obligatoria.", requestId, "request"));
            if (ContainsHeaderInjection(request))
                return BadRequest(Error("EMAIL_CRLF_INVALID", "La solicitud contiene caracteres no permitidos.", requestId));
            var alias = claimValidation.ValidateClaim<string>("defaulalias");
            if (!alias.Success || string.IsNullOrWhiteSpace(alias.ClaimValue))
                return Unauthorized(Error("EMAIL_ALIAS_REQUIRED", "No fue posible validar el contexto de la solicitud.", requestId, "defaulalias"));
            var empresa = claimValidation.ValidateClaim<string>("emp");
            if (!empresa.Success || !int.TryParse(empresa.ClaimValue, out var idEmpresa) || idEmpresa <= 0)
                return Unauthorized(Error("EMAIL_COMPANY_REQUIRED", "No fue posible validar la empresa de la solicitud.", requestId, "emp"));
            ct.ThrowIfCancellationRequested();
            var result = await service.SolicitarEnvioAsync(request, alias.ClaimValue.Trim(), idEmpresa, ct).ConfigureAwait(false);
            result.meta ??= new AppMeta();
            result.meta.RequestId ??= requestId;
            result.meta.Metadata ??= new Dictionary<string, object?>();
            result.meta.Metadata["xRequestId"] = externalRequestId;
            activity?.SetTag("metaStatus", result.meta.Status ?? (result.success ? "OK" : "ERROR"));
            if (result.success) return Ok(result);
            return StatusCode(MapStatus(result), result);
        }
        catch (OperationCanceledException) { activity?.SetStatus(ActivityStatusCode.Error, "EMAIL_CANCELLED"); throw; }
        catch (Exception ex)
        {
            logger.LogError(ex, "Email controller request failed. requestId={RequestId} xRequestId={XRequestId} errorCode={ErrorCode}", requestId, externalRequestId, "EMAIL_CONTROLLER_ERROR");
            activity?.SetStatus(ActivityStatusCode.Error, "EMAIL_CONTROLLER_ERROR");
            return StatusCode(StatusCodes.Status500InternalServerError, Error("EMAIL_CONTROLLER_ERROR", "No fue posible procesar la solicitud de correo.", requestId));
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
        if (code.Contains("QUEUE", StringComparison.OrdinalIgnoreCase) || code.Contains("REPOSITORY", StringComparison.OrdinalIgnoreCase)) return StatusCodes.Status503ServiceUnavailable;
        if (code is "EMAIL_RECIPIENT_VALIDATION_UNAVAILABLE") return StatusCodes.Status503ServiceUnavailable;
        if (code.Contains("INVALID", StringComparison.OrdinalIgnoreCase) || code.Contains("REQUIRED", StringComparison.OrdinalIgnoreCase) || code is "EMAIL_RECIPIENT_NOT_FOUND") return StatusCodes.Status400BadRequest;
        return StatusCodes.Status500InternalServerError;
    }

    private static AppResponses<RespuestaEnvioCorreoDto> Error(string code, string userMessage, string requestId, string? field = null) => new()
    {
        success = false, message = userMessage, data = default!,
        meta = new AppMeta { RequestId = requestId, Status = "ERROR", ResponseVersion = "1.0" },
        errors = [new AppError { Type = "Validation", Code = code, UserMessage = userMessage, Message = userMessage, Field = field }]
    };
}
