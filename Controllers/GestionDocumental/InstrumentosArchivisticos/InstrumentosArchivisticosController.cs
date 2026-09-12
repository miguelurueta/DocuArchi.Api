using System.Diagnostics;
using MiApp.DTOs.DTOs.GestorDocumental.InstrumentosArchivisticos;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.InstrumentosArchivisticos;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DocuArchi.Api.Controllers.GestorDocumental.InstrumentosArchivisticos;

[ApiController]
[Authorize]
[Route("api/instrumentos-archivisticos")]
public sealed class InstrumentosArchivisticosController : ControllerBase
{
    private readonly IInstrumentoArchivisticoQueryService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<InstrumentosArchivisticosController> _logger;
    public InstrumentosArchivisticosController(
        IInstrumentoArchivisticoQueryService service,
        ICurrentUserService currentUser,
        ILogger<InstrumentosArchivisticosController> logger)
    {
        _service = service;
        _currentUser = currentUser;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(AppResponses<IReadOnlyList<InstrumentoArchivisticoDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<AppResponses<IReadOnlyList<InstrumentoArchivisticoDto>>>> List([FromQuery] bool soloActivos = true)
    {
        var response = await _service.ListAsync(soloActivos, _currentUser.ResolveAliasOrThrow(), HttpContext.TraceIdentifier);
        return StatusCode(ResolveHttpStatus(response), response);
    }

    [HttpGet("{idInstrumento:int}")]
    [ProducesResponseType(typeof(AppResponses<InstrumentoArchivisticoDetalleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<AppResponses<InstrumentoArchivisticoDetalleDto>>> GetById([FromRoute] int idInstrumento)
    {
        var response = await _service.GetByIdAsync(idInstrumento, _currentUser.ResolveAliasOrThrow(), HttpContext.TraceIdentifier);
        return StatusCode(ResolveHttpStatus(response), response);
    }

    [HttpGet("{idInstrumento:int}/series")]
    [ProducesResponseType(typeof(AppResponses<IReadOnlyList<InstrumentoArchivisticoSerieDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppResponses<IReadOnlyList<InstrumentoArchivisticoSerieDto>>>> ListSeries([FromRoute] int idInstrumento)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = HttpContext.TraceIdentifier;
        var usuarioId = _currentUser.ResolveUsuarioIdOrThrow();
        var response = await _service.ListSeriesAsync(idInstrumento, _currentUser.ResolveAliasOrThrow(), requestId);
        var status = ResolveHttpStatus(response);
        var code = response.success
            ? "NONE"
            : response.errors?.OfType<MiApp.DTOs.DTOs.Errors.AppError>().FirstOrDefault()?.Code ?? "UNKNOWN";
        _logger.LogInformation(
            "Instrument series query completed. RequestId {RequestId} UsuarioId {UsuarioId} Stage {Stage} DurationMs {DurationMs} ErrorCode {ErrorCode} HttpStatus {HttpStatus}",
            requestId, usuarioId, "completed", stopwatch.Elapsed.TotalMilliseconds, code, status);
        return StatusCode(status, response);
    }

    private static int ResolveHttpStatus<T>(AppResponses<T> response)
    {
        if (response.success) return StatusCodes.Status200OK;
        var code = response.errors?.OfType<MiApp.DTOs.DTOs.Errors.AppError>().FirstOrDefault()?.Code;
        return InstrumentoArchivisticoErrorResponseFactory.ResolveHttpStatus(code);
    }
}
