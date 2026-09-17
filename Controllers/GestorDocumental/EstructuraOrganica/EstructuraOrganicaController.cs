using MiApp.DTOs.DTOs.GestorDocumental.Expedientes;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.Expedientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace DocuArchi.Api.Controllers.GestorDocumental.EstructuraOrganica;

[ApiController, Authorize, Route("api/estructura-organica")]
public sealed class EstructuraOrganicaController(IExpedienteCatalogService catalogs) : ControllerBase
{
    [HttpGet("organigramas")]
    [ProducesResponseType(typeof(AppResponses<IReadOnlyList<OrganigramaDto>>), 200)]
    [ProducesResponseType(typeof(AppResponses<IReadOnlyList<OrganigramaDto>>), 403)]
    [ProducesResponseType(typeof(AppResponses<IReadOnlyList<OrganigramaDto>>), 500)]
    public async Task<ActionResult<AppResponses<IReadOnlyList<OrganigramaDto>>>> Organigramas()
    { var result = await catalogs.ListOrganigramasAsync(Alias(), HttpContext.TraceIdentifier); return Map(result); }

    [HttpGet("areas")]
    [ProducesResponseType(typeof(AppResponses<IReadOnlyList<AreaOrganicaDto>>), 200)]
    [ProducesResponseType(typeof(AppResponses<IReadOnlyList<AreaOrganicaDto>>), 400)]
    [ProducesResponseType(typeof(AppResponses<IReadOnlyList<AreaOrganicaDto>>), 403)]
    [ProducesResponseType(typeof(AppResponses<IReadOnlyList<AreaOrganicaDto>>), 500)]
    public async Task<ActionResult<AppResponses<IReadOnlyList<AreaOrganicaDto>>>> Areas([FromQuery] int organigramaId)
    { var result = await catalogs.ListAreasAsync(organigramaId, Alias(), HttpContext.TraceIdentifier); return Map(result); }

    private ActionResult<AppResponses<T>> Map<T>(AppResponses<T> result) => result.success
        ? Ok(result)
        : result.message == "ORGANIGRAMA_ID_INVALID" ? BadRequest(result)
        : result.message.Contains("FORBIDDEN", StringComparison.Ordinal) ? StatusCode(403, result)
        : StatusCode(500, result);
    private string Alias() => User.FindFirst("defaulalias")?.Value ?? string.Empty;
}
