using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DocuArchi.Api.Infrastructure.Filters;
using MiApp.DTOs.DTOs.Radicacion.Radicados.Autocomplete;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Repository.Repositorio.DataAccess;
using MiApp.Services.Service.Radicacion.Radicados.Autocomplete;
using MiApp.Services.Service.Radicacion.Radicados.Autocomplete.Errors;
using MiApp.Services.Service.Radicacion.ResolucionDocumental.Errors;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
namespace DocuArchi.Api.Controllers.Radicacion.Radicados.Autocomplete;

/// <summary>Frontera autenticada de búsqueda de consecutivos; no contiene SQL ni acceso directo a repositorios.</summary>
[ApiController, Authorize, Route("api/radicacion/radicados")]
public sealed class SugerenciasRadicadosController : ControllerBase
{
    private readonly IBusquedaRadicadosService _servicio;
    private readonly IClaimValidationService _claims;
    private readonly ErroresSugerenciasRadicados _errores;

    /// <summary>Compone caso de uso, claims existentes y fábrica de errores específicos de la capacidad.</summary>
    /// <param name="servicio">Caso de uso de lectura documental.</param>
    /// <param name="claims">Validador del contexto autenticado actual.</param>
    /// <param name="errores">Proyección de errores del contrato nuevo.</param>
    public SugerenciasRadicadosController(IBusquedaRadicadosService servicio, IClaimValidationService claims, ErroresSugerenciasRadicados errores)
    { _servicio = servicio; _claims = claims; _errores = errores; }

    /// <summary>Consulta sugerencias sólo para la plantilla operativa seleccionada, sin mutaciones documentales.</summary>
    /// <param name="solicitud">Request camelCase del contrato v1.</param>
    /// <param name="cancellationToken">Cancelación de la solicitud HTTP.</param>
    /// <returns>AppResponses serializado con estado del catálogo; seguridad conserva 401/403 existentes.</returns>
    [HttpPost("autocomplete"), ValidacionSolicitudSugerenciasRadicados]
    [ProducesResponseType(typeof(AppResponses<ResultadoSugerenciasRadicadosDto>), 200)]
    [ProducesResponseType(typeof(AppResponses<ResultadoSugerenciasRadicadosDto>), 400)]
    [ProducesResponseType(typeof(AppResponses<ResultadoSugerenciasRadicadosDto>), 404)]
    [ProducesResponseType(typeof(AppResponses<ResultadoSugerenciasRadicadosDto>), 409)]
    [ProducesResponseType(typeof(AppResponses<ResultadoSugerenciasRadicadosDto>), 500)]
    [ProducesResponseType(typeof(AppResponses<ResultadoSugerenciasRadicadosDto>), 503)]
    [ProducesResponseType(typeof(void), 401), ProducesResponseType(typeof(void), 403)]
    public async Task<IActionResult> ConsultarSugerenciasRadicadosAsync(
        [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Disallow), System.ComponentModel.DataAnnotations.Required]
        SolicitudSugerenciasRadicadosDto? solicitud, CancellationToken cancellationToken)
    {
        var requestId = ErroresResolucionDocumental.NormalizarCorrelacionDocumental(HttpContext.TraceIdentifier);
        int? actor = null;
        Response.Headers.CacheControl = "no-store";
        Response.Headers["X-Request-Id"] = requestId;
        try
        {
            var alias = _claims.ValidateClaim<string>("defaulalias");
            var usuario = _claims.ValidateClaim<string>("usuarioid");
            if (!alias.Success || string.IsNullOrWhiteSpace(alias.ClaimValue) || !usuario.Success
                || !int.TryParse(usuario.ClaimValue, out var usuarioId) || usuarioId <= 0)
                return BadRequest(_errores.CrearErrorSugerenciasRadicados("CONTEXTO_INVALIDO", requestId, codigoTecnico: "CONTEXTO_NO_ACREDITADO"));
            actor = usuarioId;
            var resultado = await _servicio.BuscarSugerenciasRadicadosAsync(solicitud, alias.ClaimValue, usuarioId, requestId, cancellationToken);
            return StatusCode(ErroresSugerenciasRadicados.ConsultarEstadoHttpSugerencias(resultado), resultado);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception causa)
        {
            var diagnostico = causa is DatabaseOperationDiagnosticException fallo ? fallo.Diagnostic : new DatabaseOperationDiagnostics();
            diagnostico.Capture("frontera-sugerencias-radicados", causa);
            var respuesta = _errores.CrearErrorSugerenciasRadicados("CONSULTA_FALLIDA", requestId, diagnostico, usuarioId: actor);
            return StatusCode(ErroresSugerenciasRadicados.ConsultarEstadoHttpSugerencias(respuesta), respuesta);
        }
    }
}
