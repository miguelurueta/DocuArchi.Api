using System.Globalization;
using DocuArchi.Api.Infrastructure.TipologiasDocumentales.Actualizacion;
using MiApp.DTOs.DTOs.GestorDocumental.TipologiasDocumentales.Actualizacion;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.TipologiasDocumentales.Actualizacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.GestorDocumental.TipologiasDocumentales.Actualizacion.WorkflowRadicacion;

/// <summary>Expone el comando aprobado de actualización, sin reglas de negocio ni SQL en el Controller.</summary>
[ApiController]
[Authorize]
[Route("api/gestor-documental/tipologias-documentales/actualizacion/workflow-radicacion")]
[ServiceFilter(typeof(FronteraHttpActualizacionTipologia), Order = -10000)]
public sealed class ActualizarTipologiaWorkflowRadicacionController : ControllerBase
{
    private readonly IActualizacionTipologiaWorkflowRadicacionService _servicio;
    private readonly IErrorActualizacionTipologiaFactory _errores;

    /// <summary>Recibe el caso de uso y el diagnóstico propio; no depende de almacenamiento.</summary>
    /// <param name="servicio">Caso de uso que acredita persistencia SQL/XML.</param>
    /// <param name="errores">Catálogo y proyección segura de errores de ruta.</param>
    public ActualizarTipologiaWorkflowRadicacionController(IActualizacionTipologiaWorkflowRadicacionService servicio,
        IErrorActualizacionTipologiaFactory errores) { _servicio = servicio; _errores = errores; }

    /// <summary>Actualiza la tipología existente en WF/RA; null explícito solicita retirada condicionada.</summary>
    /// <param name="idDocumento">ID de gabinete como texto de ruta para normalizar también valores inválidos a 400.</param>
    /// <param name="solicitud">Cuerpo convertido por el contrato específico, con selección presente.</param>
    /// <param name="cancelacion">Cancelación de petición; la resolución de efectos iniciados pertenece al coordinador.</param>
    /// <returns>Respuesta JSON contractual con valores persistidos o diagnóstico seguro y correlacionado.</returns>
    [HttpPatch("{idDocumento}")]
    [ProducesResponseType(typeof(AppResponses<ActualizarTipologiaDocumentalResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppResponses<ActualizarTipologiaDocumentalResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AppResponses<ActualizarTipologiaDocumentalResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(AppResponses<ActualizarTipologiaDocumentalResponseDto>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(AppResponses<ActualizarTipologiaDocumentalResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AppResponses<ActualizarTipologiaDocumentalResponseDto>), StatusCodes.Status406NotAcceptable)]
    [ProducesResponseType(typeof(AppResponses<ActualizarTipologiaDocumentalResponseDto>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(AppResponses<ActualizarTipologiaDocumentalResponseDto>), StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(typeof(AppResponses<ActualizarTipologiaDocumentalResponseDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ActualizarTipologiaWorkflowRadicacionAsync([FromRoute] string idDocumento,
        [SolicitudActualizacionTipologiaDesdeJson] ActualizarTipologiaWorkflowRadicacionRequestDto? solicitud, CancellationToken cancelacion)
    {
        var requestId = FronteraHttpActualizacionTipologia.ObtenerCorrelacionSegura(HttpContext);
        if (!long.TryParse(idDocumento, NumberStyles.None, CultureInfo.InvariantCulture, out var id) || id <= 0 || solicitud is null)
        {
            var error = _errores.CrearRespuestaError("TIPDOC-UPD-REQUEST-INVALID", requestId, "binding", "LeerSolicitudActualizacionTipologia", 0, campo: "idDocumento");
            return FronteraHttpActualizacionTipologia.CrearResultadoJson(error.EstadoHttp, error.Respuesta);
        }
        var resultado = await _servicio.ActualizarTipologiaWorkflowRadicacionAsync(solicitud, id, requestId, cancelacion).ConfigureAwait(false);
        return FronteraHttpActualizacionTipologia.CrearResultadoJson(resultado.EstadoHttp, resultado.Respuesta);
    }
}
