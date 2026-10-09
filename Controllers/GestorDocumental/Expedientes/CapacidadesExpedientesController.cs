using MiApp.DTOs.DTOs.Errors;
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
    [ProducesResponseType(typeof(AppResponses<ExpedienteCapabilitiesDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AppResponses<ExpedienteCapabilitiesDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(AppResponses<ExpedienteCapabilitiesDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(AppResponses<ExpedienteCapabilitiesDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AppResponses<ExpedienteCapabilitiesDto>>> Get(
        CancellationToken cancellationToken)
    {
        var response = await service.CapabilitiesAsync(
            HttpContext.TraceIdentifier, cancellationToken);
        if (response.success) return Ok(response);

        var code = response.errors?
            .OfType<AppError>()
            .Select(error => error.Code)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        return StatusCode(
            ExpedienteCapabilitiesErrorResponseFactory.ResolveHttpStatus(code),
            response);
    }
}
