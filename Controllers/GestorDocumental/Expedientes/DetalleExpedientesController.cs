using MiApp.DTOs.DTOs.GestorDocumental.Expedientes;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.Expedientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.GestorDocumental.Expedientes;

[ApiController, Authorize, Route("api/gestion-expedientes")]
public sealed class DetalleExpedientesController(IExpedienteService service) : ControllerBase
{
    [HttpGet("{idExpediente:int}")]
    [ProducesResponseType(typeof(AppResponses<ExpedienteDetalleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<AppResponses<ExpedienteDetalleDto>>> Get(int idExpediente)
    {
        var result = await service.GetByIdAsync(idExpediente, User.FindFirst("defaulalias")?.Value ?? "", HttpContext.TraceIdentifier);
        if (result.success) return Ok(result);
        return result.message == "EXPEDIENTE_NOT_FOUND" ? NotFound(result) : BadRequest(result);
    }
}
