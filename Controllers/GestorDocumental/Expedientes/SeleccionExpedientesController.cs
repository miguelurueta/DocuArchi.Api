using MiApp.DTOs.DTOs.GestorDocumental.Expedientes;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.Expedientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.GestorDocumental.Expedientes;

[ApiController, Authorize, Route("api/gestion-expedientes")]
public sealed class SeleccionExpedientesController(IExpedienteService service) : ControllerBase
{
    [HttpPost("{idExpediente:int}/validar-seleccion")]
    [ProducesResponseType(typeof(AppResponses<ValidarSeleccionExpedienteResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<AppResponses<ValidarSeleccionExpedienteResponseDto>>> Validate(int idExpediente, [FromBody] ValidarSeleccionExpedienteRequestDto request)
    {
        var result = await service.ValidateAsync(idExpediente, request, User.FindFirst("defaulalias")?.Value ?? "", HttpContext.TraceIdentifier);
        return result.success ? Ok(result) : BadRequest(result);
    }
}
