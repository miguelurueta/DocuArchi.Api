using DocuArchi.Api.Infrastructure.Filters;
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
    ICrearExpedienteUseCase createExpediente,
    IExpedienteService expedientes,
    IExpedienteEditModelService editModel,
    IEditarExpedienteUseCase editExpediente) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(AppResponses<CrearExpedienteResponseDto>), 201)]
    [ProducesResponseType(typeof(AppResponses<CrearExpedienteResponseDto>), 400)]
    [ProducesResponseType(typeof(AppResponses<CrearExpedienteResponseDto>), 401)]
    [ProducesResponseType(typeof(AppResponses<CrearExpedienteResponseDto>), 403)]
    [ProducesResponseType(typeof(AppResponses<CrearExpedienteResponseDto>), 404)]
    [ProducesResponseType(typeof(AppResponses<CrearExpedienteResponseDto>), 422)]
    [ProducesResponseType(typeof(AppResponses<CrearExpedienteResponseDto>), 500)]
    [Microsoft.AspNetCore.Http.EndpointDescription("""
        Crea un expediente con identidad y alcance resueltos desde el contexto autenticado.
        Ejemplo de request (IDs ilustrativos, utilizar catálogos autorizados):
        { "nombreExpediente": "Expediente de prueba", "idTipoExpediente": 7, "idTipoUnidadDocumental": 1,
          "idInstrumento": 3, "idCiclo": 4, "idOrganigrama": 1, "idArea": 46, "idSerie": 2,
          "fechaExtremaInicial": "2026-10-21" }
        Éxito 201: { "success": true, "message": "EXPEDIENTE_CREATED", "data": { "idExpediente": 1,
          "codigoUnico": "Expediente de prueba", "codigoCorto": "26-2-141-1", "consecutivo": 1 },
          "errors": [], "meta": { "Status": "success", "RequestId": "correlation-id" } }
        Fallo técnico 500, contrato aditivo:
        { "success": false, "message": "EXPEDIENTE_CREATE_FAILED", "data": null,
          "errors": [{ "Type": "Infrastructure", "Code": "EXPEDIENTE_CREATE_FAILED", "Field": "create",
          "Message": "No fue posible crear el expediente.", "UserMessage": "No fue posible crear el expediente." }],
          "meta": { "Status": "error", "RequestId": "correlation-id", "Metadata": { "Stage": "transaction",
          "Operation": "insert-expediente", "TechnicalCode": "EXPEDIENTE_INSERT_FAILED",
          "TechnicalDetail": "No fue posible insertar el expediente." } } }
        Las operaciones distinguen conexión, inicio, lectura y actualización del consecutivo, inserción, auditoría,
        XML y commit. Metadata puede indicar RollbackFailed o CompensationFailed sin sustituir la causa principal.
        Mostrar errors[].UserMessage al usuario; message conserva su código público y su uso para resolver HTTP status.
        Respuestas: 201 creado; 400 validación/contexto; 401 sin autenticación; 403 autorización;
        404 instrumento; 422 clasificación/retención/referencia; 500 infraestructura.
        El acceso denegado a archivos en XML es infraestructura 500; una denegación real de alcance conserva 403.
        """)]
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
            "EXPEDIENTE_NAME_INVALID" or "EXPEDIENTE_ALIAS_INVALID" or "EXPEDIENTE_RESPONSIBLE_INVALID" or "EXPEDIENTE_REQUEST_INVALID" or "EXPEDIENTE_DATES_INVALID" or "EXPEDIENTE_FOLIOS_INVALID" or "EXPEDIENTE_CONTEXT_INVALID" => StatusCodes.Status400BadRequest,
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

    [HttpPost("campos/autocomplete")]
    [ProducesResponseType(typeof(AppResponses<ExpedienteAutocompleteResponseDto>), 200)]
    [ProducesResponseType(typeof(AppResponses<ExpedienteAutocompleteResponseDto>), 400)]
    [ProducesResponseType(typeof(AppResponses<ExpedienteAutocompleteResponseDto>), 403)]
    [ProducesResponseType(typeof(AppResponses<ExpedienteAutocompleteResponseDto>), 500)]
    [ExpedienteAutocompleteValidationResponse]
    public async Task<ActionResult<AppResponses<ExpedienteAutocompleteResponseDto>>> Autocomplete(
        [FromBody] ExpedienteAutocompleteRequestDto request)
    {
        var result = await expedientes.AutocompleteAsync(request, Alias(), HttpContext.TraceIdentifier);
        var status = result.success ? StatusCodes.Status200OK : result.message switch
        {
            "EXPEDIENTE_AUTOCOMPLETE_TEXT_INVALID" => StatusCodes.Status400BadRequest,
            "EXPEDIENTE_CREATE_FORBIDDEN" or "EXPEDIENTE_SCOPE_FORBIDDEN" => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };
        return StatusCode(status, result);
    }

    [HttpGet("{idExpediente:int}/edit-model")]
    [ProducesResponseType(typeof(AppResponses<ExpedienteEditModelDto>), 200)]
    [ProducesResponseType(typeof(AppResponses<ExpedienteEditModelDto>), 400)]
    [ProducesResponseType(typeof(AppResponses<ExpedienteEditModelDto>), 403)]
    [ProducesResponseType(typeof(AppResponses<ExpedienteEditModelDto>), 404)]
    [ProducesResponseType(typeof(AppResponses<ExpedienteEditModelDto>), 409)]
    [ProducesResponseType(typeof(AppResponses<ExpedienteEditModelDto>), 422)]
    [ProducesResponseType(typeof(AppResponses<ExpedienteEditModelDto>), 500)]
    public async Task<ActionResult<AppResponses<ExpedienteEditModelDto>>> GetEditModel(
        [FromRoute] int idExpediente,
        CancellationToken cancellationToken)
    {
        var result = await editModel.GetAsync(idExpediente, Alias(), HttpContext.TraceIdentifier, cancellationToken);
        return StatusCode(ResolveStatus(result), result);
    }

    [HttpPut("{idExpediente:int}")]
    [ProducesResponseType(typeof(AppResponses<EditarExpedienteResponseDto>), 200)]
    [ProducesResponseType(typeof(AppResponses<EditarExpedienteResponseDto>), 400)]
    [ProducesResponseType(typeof(AppResponses<EditarExpedienteResponseDto>), 401)]
    [ProducesResponseType(typeof(AppResponses<EditarExpedienteResponseDto>), 403)]
    [ProducesResponseType(typeof(AppResponses<EditarExpedienteResponseDto>), 404)]
    [ProducesResponseType(typeof(AppResponses<EditarExpedienteResponseDto>), 409)]
    [ProducesResponseType(typeof(AppResponses<EditarExpedienteResponseDto>), 422)]
    [ProducesResponseType(typeof(AppResponses<EditarExpedienteResponseDto>), 500)]
    public async Task<ActionResult<AppResponses<EditarExpedienteResponseDto>>> Update(
        [FromRoute] int idExpediente,
        [FromBody] EditarExpedienteRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await editExpediente.ExecuteAsync(idExpediente, request,
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            HttpContext.TraceIdentifier, cancellationToken);
        return StatusCode(ResolveStatus(result), result);
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

    private static int ResolveStatus<T>(AppResponses<T> result)
    {
        if (result.success) return StatusCodes.Status200OK;
        return result.message switch
        {
            "EXPEDIENTE_EDIT_FORBIDDEN" or "EXPEDIENTE_EDIT_SCOPE_FORBIDDEN" => StatusCodes.Status403Forbidden,
            "EXPEDIENTE_EDIT_NOT_FOUND" or "EXPEDIENTE_EDIT_INSTRUMENT_NOT_FOUND" => StatusCodes.Status404NotFound,
            "EXPEDIENTE_EDIT_AUTOREG_BLOCKED" or "EXPEDIENTE_EDIT_STATE_BLOCKED" or "EXPEDIENTE_EDIT_OWNER_BLOCKED" or "EXPEDIENTE_EDIT_CONCURRENCY" => StatusCodes.Status409Conflict,
            "EXPEDIENTE_EDIT_CLASSIFICATION_INVALID" or "EXPEDIENTE_EDIT_RETENTION_NOT_CONFIGURED" or "EXPEDIENTE_EDIT_REFERENCE_INVALID" or "EXPEDIENTE_EDIT_ORGANIZATION_INVALID" or "EXPEDIENTE_EDIT_RESPONSIBLE_INVALID" => StatusCodes.Status422UnprocessableEntity,
            "EXPEDIENTE_EDIT_REQUEST_INVALID" or "EXPEDIENTE_EDIT_NAME_INVALID" or "EXPEDIENTE_EDIT_ALIAS_INVALID" or "EXPEDIENTE_EDIT_DATES_INVALID" or "EXPEDIENTE_EDIT_FOLIOS_INVALID" or "EXPEDIENTE_EDIT_CONTEXT_INVALID" or "EXPEDIENTE_EDIT_ROW_VERSION_REQUIRED" or "EXPEDIENTE_EDIT_ROW_VERSION_INVALID" => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}
