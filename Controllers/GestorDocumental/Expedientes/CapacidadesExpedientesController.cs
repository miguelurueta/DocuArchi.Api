using MiApp.DTOs.DTOs.GestorDocumental.Expedientes;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.Expedientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.GestorDocumental.Expedientes;

[ApiController, Authorize, Route("api/gestion-expedientes")]
public sealed class CapacidadesExpedientesController(IExpedienteService service) : ControllerBase
{
    [HttpGet("capabilities")]
    [ProducesResponseType(typeof(AppResponses<ExpedienteCapabilitiesDto>), StatusCodes.Status200OK)]
    public ActionResult<AppResponses<ExpedienteCapabilitiesDto>> Get() => Ok(service.Capabilities(HttpContext.TraceIdentifier));
}
