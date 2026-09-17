using MiApp.DTOs.DTOs.GestorDocumental.Expedientes;
using MiApp.DTOs.DTOs.GestorDocumental.InstrumentosArchivisticos;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.Expedientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace DocuArchi.Api.Controllers.GestorDocumental.Archivo;

[ApiController, Authorize, Route("api/archivo")]
public sealed class ArchivoCatalogosController(IExpedienteCatalogService catalogs) : ControllerBase
{
    [ProducesResponseType(typeof(AppResponses<IReadOnlyList<FondoDto>>), 200)]
    [ProducesResponseType(typeof(AppResponses<IReadOnlyList<FondoDto>>), 500)]
    [HttpGet("fondos")] public Task<ActionResult<AppResponses<IReadOnlyList<FondoDto>>>> Fondos() => Execute(catalogs.ListFondosAsync(Alias(), HttpContext.TraceIdentifier));
    [ProducesResponseType(typeof(AppResponses<IReadOnlyList<CicloArchivoDto>>), 200)]
    [ProducesResponseType(typeof(AppResponses<IReadOnlyList<CicloArchivoDto>>), 500)]
    [HttpGet("ciclos")] public Task<ActionResult<AppResponses<IReadOnlyList<CicloArchivoDto>>>> Ciclos() => Execute(catalogs.ListCyclesAsync(Alias(), HttpContext.TraceIdentifier));
    [ProducesResponseType(typeof(AppResponses<IReadOnlyList<UnidadConservacionDto>>), 200)]
    [ProducesResponseType(typeof(AppResponses<IReadOnlyList<UnidadConservacionDto>>), 500)]
    [HttpGet("unidades-conservacion")] public Task<ActionResult<AppResponses<IReadOnlyList<UnidadConservacionDto>>>> Unidades() => Execute(catalogs.ListConservationUnitTypesAsync(Alias(), HttpContext.TraceIdentifier));
    [ProducesResponseType(typeof(AppResponses<IReadOnlyList<TipoUnidadDocumentalDto>>), 200)]
    [ProducesResponseType(typeof(AppResponses<IReadOnlyList<TipoUnidadDocumentalDto>>), 500)]
    [HttpGet("tipos-unidad-documental")] public Task<ActionResult<AppResponses<IReadOnlyList<TipoUnidadDocumentalDto>>>> TiposUnidad() => Execute(catalogs.ListDocumentUnitTypesAsync(Alias(), HttpContext.TraceIdentifier));
    private async Task<ActionResult<AppResponses<T>>> Execute<T>(Task<AppResponses<T>> operation)
    { var result = await operation; return result.success ? Ok(result) : StatusCode(500, result); }
    private string Alias() => User.FindFirst("defaulalias")?.Value ?? string.Empty;
}
