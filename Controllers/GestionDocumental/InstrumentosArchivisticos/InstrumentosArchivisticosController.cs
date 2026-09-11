using MiApp.DTOs.DTOs.GestorDocumental.InstrumentosArchivisticos;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.InstrumentosArchivisticos;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.GestorDocumental.InstrumentosArchivisticos;

[ApiController]
[Authorize]
[Route("api/instrumentos-archivisticos")]
public sealed class InstrumentosArchivisticosController : ControllerBase
{
    private readonly IInstrumentoArchivisticoQueryService _service;
    private readonly ICurrentUserService _currentUser;
    public InstrumentosArchivisticosController(IInstrumentoArchivisticoQueryService service, ICurrentUserService currentUser) { _service = service; _currentUser = currentUser; }

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

    private static int ResolveHttpStatus<T>(AppResponses<T> response)
    {
        if (response.success) return StatusCodes.Status200OK;
        var code = response.errors?.OfType<MiApp.DTOs.DTOs.Errors.AppError>().FirstOrDefault()?.Code;
        return InstrumentoArchivisticoErrorResponseFactory.ResolveHttpStatus(code);
    }
}
