using MiApp.DTOs.DTOs.GestorDocumental.Expedientes;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.Expedientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.GestorDocumental.Expedientes;

[ApiController, Authorize, Route("api/gestion-expedientes")]
public sealed class BusquedaExpedientesController(IExpedienteService service) : ControllerBase
{
    [HttpPost("search")]
    [ProducesResponseType(typeof(AppResponses<PagedResultDto<ExpedienteSearchResultDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<AppResponses<PagedResultDto<ExpedienteSearchResultDto>>>> Search([FromBody] ExpedienteSearchRequestDto request)
    {
        var result = await service.SearchAsync(request, Alias(), HttpContext.TraceIdentifier);
        return result.success ? Ok(result) : BadRequest(result);
    }
    private string Alias() => User.FindFirst("defaulalias")?.Value ?? string.Empty;
}
