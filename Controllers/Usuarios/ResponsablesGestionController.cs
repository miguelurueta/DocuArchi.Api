using MiApp.DTOs.DTOs.GestorDocumental.Expedientes;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.Expedientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace DocuArchi.Api.Controllers.Usuarios;

[ApiController, Authorize, Route("api/usuarios/responsables")]
public sealed class ResponsablesGestionController(IExpedienteCatalogService catalogs) : ControllerBase
{
    [HttpGet("search")]
    [ProducesResponseType(typeof(AppResponses<IReadOnlyList<ResponsableGestionDto>>), 200)]
    [ProducesResponseType(typeof(AppResponses<IReadOnlyList<ResponsableGestionDto>>), 400)]
    [ProducesResponseType(typeof(AppResponses<IReadOnlyList<ResponsableGestionDto>>), 500)]
    public async Task<ActionResult<AppResponses<IReadOnlyList<ResponsableGestionDto>>>> Search([FromQuery] string texto, [FromQuery] int limit = 20)
    {
        var result = await catalogs.SearchResponsablesAsync(texto, limit, Alias(), HttpContext.TraceIdentifier);
        return result.success ? Ok(result)
            : result.message == "RESPONSABLE_SEARCH_TEXT_INVALID" ? BadRequest(result)
            : StatusCode(500, result);
    }
    private string Alias() => User.FindFirst("defaulalias")?.Value ?? string.Empty;
}
