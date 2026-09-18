using MiApp.DTOs.DTOs.GestorDocumental.Expedientes;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.Expedientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace DocuArchi.Api.Controllers.GestorDocumental.Expedientes;

[ApiController, Authorize, Route("api/gestion-expedientes")]
public sealed class GestionExpedientesCreateModelController(
    IExpedienteCreateModelService createModel,
    IExpedienteCatalogService catalogs,
    ICrearExpedienteUseCase createExpediente) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(AppResponses<CrearExpedienteResponseDto>), 201)]
    [ProducesResponseType(typeof(AppResponses<CrearExpedienteResponseDto>), 400)]
    [ProducesResponseType(typeof(AppResponses<CrearExpedienteResponseDto>), 401)]
    [ProducesResponseType(typeof(AppResponses<CrearExpedienteResponseDto>), 403)]
    [ProducesResponseType(typeof(AppResponses<CrearExpedienteResponseDto>), 404)]
    [ProducesResponseType(typeof(AppResponses<CrearExpedienteResponseDto>), 422)]
    [ProducesResponseType(typeof(AppResponses<CrearExpedienteResponseDto>), 500)]
    public async Task<ActionResult<AppResponses<CrearExpedienteResponseDto>>> Create(
        [FromBody] CrearExpedienteRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await createExpediente.ExecuteAsync(request,
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            HttpContext.TraceIdentifier, cancellationToken);
        if (result.success) return StatusCode(StatusCodes.Status201Created, result);
        var status = result.message switch
        {
            "EXPEDIENTE_CREATE_FORBIDDEN" or "EXPEDIENTE_SCOPE_FORBIDDEN" => StatusCodes.Status403Forbidden,
            "EXPEDIENTE_INSTRUMENT_NOT_FOUND" => StatusCodes.Status404NotFound,
            "EXPEDIENTE_CLASSIFICATION_INVALID" or "EXPEDIENTE_REFERENCE_INVALID" or "EXPEDIENTE_RETENTION_NOT_CONFIGURED" => StatusCodes.Status422UnprocessableEntity,
            "EXPEDIENTE_NAME_INVALID" or "EXPEDIENTE_ALIAS_INVALID" or "EXPEDIENTE_REQUEST_INVALID" or "EXPEDIENTE_DATES_INVALID" or "EXPEDIENTE_FOLIOS_INVALID" or "EXPEDIENTE_CONTEXT_INVALID" => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
        return StatusCode(status, result);
    }

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
