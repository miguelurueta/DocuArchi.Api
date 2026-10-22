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
    [EndpointDescription("Consulta de solo lectura con las 31 propiedades declaradas del detalle. Las referencias y fechas ausentes permanecen null; rowVersion es null si falta fecha_registro y no habilita edición sin versión. En fallos técnicos se conserva HTTP 400 y EXPEDIENTE_READ_FAILED; meta.Metadata incluye Stage, Operation, TechnicalCode y TechnicalDetail seguros disponibles, correlacionados por meta.RequestId. El mensaje funcional está en errors[].UserMessage.")]
    [ProducesResponseType(typeof(AppResponses<ExpedienteDetalleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppResponses<ExpedienteDetalleDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AppResponses<ExpedienteDetalleDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AppResponses<ExpedienteDetalleDto>>> Get(int idExpediente)
    {
        var result = await service.GetByIdAsync(idExpediente, User.FindFirst("defaulalias")?.Value ?? "", HttpContext.TraceIdentifier);
        if (result.success) return Ok(result);
        return result.message == "EXPEDIENTE_NOT_FOUND" ? NotFound(result) : BadRequest(result);
    }
}
