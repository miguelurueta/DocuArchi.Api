using MiApp.DTOs.DTOs.GestorDocumental.Expedientes;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.Expedientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace DocuArchi.Api.Controllers.GestorDocumental.Expedientes;

[ApiController, Authorize, Route("api/gestion-expedientes")]
public sealed class GestionExpedientesCreateModelController(IExpedienteCreateModelService createModel, IExpedienteCatalogService catalogs) : ControllerBase
{
    [HttpGet("create-model")]
    [ProducesResponseType(typeof(AppResponses<ExpedienteCreateModelDto>), 200)]
    [ProducesResponseType(typeof(AppResponses<ExpedienteCreateModelDto>), 403)]
    [ProducesResponseType(typeof(AppResponses<ExpedienteCreateModelDto>), 500)]
    public async Task<ActionResult<AppResponses<ExpedienteCreateModelDto>>> GetCreateModel()
    {
        var result = await createModel.GetAsync(Alias(), HttpContext.TraceIdentifier);
        return result.success ? Ok(result) : result.message.Contains("FORBIDDEN", StringComparison.Ordinal) ? StatusCode(403, result) : StatusCode(500, result);
    }

    [HttpGet("tipos-expediente")]
    [ProducesResponseType(typeof(AppResponses<IReadOnlyList<TipoExpedienteDto>>), 200)]
    [ProducesResponseType(typeof(AppResponses<IReadOnlyList<TipoExpedienteDto>>), 500)]
    public async Task<ActionResult<AppResponses<IReadOnlyList<TipoExpedienteDto>>>> GetTypes()
    {
        var result = await catalogs.ListExpedienteTypesAsync(Alias(), HttpContext.TraceIdentifier);
        return result.success ? Ok(result) : StatusCode(500, result);
    }
    private string Alias() => User.FindFirst("defaulalias")?.Value ?? string.Empty;
}
