using System.Diagnostics;
using MiApp.DTOs.DTOs.GestorDocumental.InstrumentosArchivisticos;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.InstrumentosArchivisticos;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DocuArchi.Api.Infrastructure.Observability;

namespace DocuArchi.Api.Controllers.GestorDocumental.InstrumentosArchivisticos;

[ApiController]
[Authorize]
[Route("api/instrumentos-archivisticos")]
public sealed class InstrumentosArchivisticosController : ControllerBase
{
    private readonly IInstrumentoArchivisticoQueryService _service;
    private readonly IInstrumentoArchivisticoCicloService _cycleService;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<InstrumentosArchivisticosController> _logger;
    private readonly IInstrumentoArchivisticoRetencionService? _retentionService;
    private readonly IRequestCorrelationContext? _correlation;
    public InstrumentosArchivisticosController(
        IInstrumentoArchivisticoQueryService service,
        IInstrumentoArchivisticoCicloService cycleService,
        ICurrentUserService currentUser,
        ILogger<InstrumentosArchivisticosController> logger,
        IInstrumentoArchivisticoRetencionService? retentionService = null,
        IRequestCorrelationContext? correlation = null)
    {
        _service = service;
        _cycleService = cycleService;
        _currentUser = currentUser;
        _logger = logger;
        _retentionService = retentionService;
        _correlation = correlation;
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

    [HttpGet("{idInstrumento:int}/series/{idSerie:int}/subseries")]
    [ProducesResponseType(typeof(AppResponses<IReadOnlyList<InstrumentoArchivisticoSubSerieDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppResponses<IReadOnlyList<InstrumentoArchivisticoSubSerieDto>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(AppResponses<IReadOnlyList<InstrumentoArchivisticoSubSerieDto>>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(AppResponses<IReadOnlyList<InstrumentoArchivisticoSubSerieDto>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AppResponses<IReadOnlyList<InstrumentoArchivisticoSubSerieDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AppResponses<IReadOnlyList<InstrumentoArchivisticoSubSerieDto>>>> ListSubSeries(
        [FromRoute] int idInstrumento,
        [FromRoute] int idSerie)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = _correlation?.RequestId
            ?? Activity.Current?.TraceId.ToString()
            ?? HttpContext.TraceIdentifier;
        var usuarioId = _currentUser.ResolveUsuarioIdOrThrow();
        var response = await _service.ListSubSeriesAsync(
            idInstrumento, idSerie, _currentUser.ResolveAliasOrThrow(), requestId);
        var status = ResolveHttpStatus(response);
        var code = response.success
            ? "NONE"
            : response.errors?.OfType<MiApp.DTOs.DTOs.Errors.AppError>().FirstOrDefault()?.Code ?? "UNKNOWN";
        _logger.LogInformation(
            "Instrument subseries query completed. RequestId {RequestId} UsuarioId {UsuarioId} IdInstrumento {IdInstrumento} IdSerie {IdSerie} Operation {Operation} Stage {Stage} DurationMs {DurationMs} ResultCount {ResultCount} ErrorCode {ErrorCode} MetaStatus {MetaStatus} HttpStatus {HttpStatus}",
            requestId, usuarioId, idInstrumento, idSerie, "list_subseries", "completed",
            stopwatch.Elapsed.TotalMilliseconds, response.success ? response.data?.Count ?? 0 : 0,
            code, response.meta?.Status ?? "unknown", status);
        return StatusCode(status, response);
    }

    [HttpGet("{idInstrumento:int}/retencion")]
    [ProducesResponseType(typeof(AppResponses<InstrumentoArchivisticoRetencionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppResponses<InstrumentoArchivisticoRetencionDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(AppResponses<InstrumentoArchivisticoRetencionDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(AppResponses<InstrumentoArchivisticoRetencionDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AppResponses<InstrumentoArchivisticoRetencionDto>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(AppResponses<InstrumentoArchivisticoRetencionDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AppResponses<InstrumentoArchivisticoRetencionDto>>> GetRetention(
        [FromRoute] int idInstrumento,
        [FromQuery] int idSerie,
        [FromQuery] int? idSubSerie,
        [FromQuery] string fechaBase)
    {
        var requestId = _correlation?.RequestId
            ?? Activity.Current?.TraceId.ToString()
            ?? HttpContext.TraceIdentifier;
        var service = _retentionService
            ?? throw new InvalidOperationException("Instrument retention service is not configured.");
        var response = await service.CalculateAsync(
            idInstrumento,
            idSerie,
            idSubSerie,
            fechaBase,
            _currentUser.ResolveAliasOrThrow(),
            _currentUser.ResolveUsuarioIdOrThrow(),
            requestId);
        return StatusCode(ResolveHttpStatus(response), response);
    }

    [HttpGet("{idInstrumento:int}/ciclos")]
    [ProducesResponseType(typeof(AppResponses<List<CicloArchivoDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AppResponses<List<CicloArchivoDto>>>> ListCycles([FromRoute] int idInstrumento)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = HttpContext.TraceIdentifier;
        var usuarioId = _currentUser.ResolveUsuarioIdOrThrow();
        var response = await _cycleService.ListCyclesAsync(
            idInstrumento,
            _currentUser.ResolveAliasOrThrow(),
            requestId);
        var status = ResolveHttpStatus(response);
        var code = response.success
            ? "NONE"
            : response.errors?.OfType<MiApp.DTOs.DTOs.Errors.AppError>().FirstOrDefault()?.Code ?? "UNKNOWN";
        _logger.LogInformation(
            "Instrument cycles query completed. RequestId {RequestId} UsuarioId {UsuarioId} Stage {Stage} DurationMs {DurationMs} ErrorCode {ErrorCode} HttpStatus {HttpStatus}",
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
