using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.GestorDocumental.Documentos.ListaDocumentosRadicados;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.Documentos.ListaDocumentosRadicados;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using MiApp.Services.Service.SessionHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.GestorDocumental.Documentos;

/// <summary>Expone listado operacional autenticado con su catálogo HTTP y diagnóstico seguro.</summary>
/// <param name="claimValidationService">Validador de identidades y aliases del JWT.</param>
/// <param name="service">Servicio de consulta y composición del menú.</param>
/// <param name="ipHelper">Obtención de dirección de petición para integración existente de borrado.</param>
/// <param name="logger">Registro opcional de fallos controlados en la frontera.</param>
[Route("api/GestorDocumental/Documentos/ListaDocumentosRadicadosOperacional")]
[ApiController]
[Authorize]
public sealed class ListaDocumentosRadicadosOperacionalController(
    IClaimValidationService claimValidationService,
    IListaDocumentosRadicadosOperacionalService service,
    IIpHelper ipHelper,
    ILogger<ListaDocumentosRadicadosOperacionalController>? logger = null) : ControllerBase
{
    /// <summary>Consulta documentos con identidad del JWT y mantiene el catálogo de estados existente.</summary>
    /// <param name="request">Referencias y filtros que el servicio debe acreditar.</param>
    /// <param name="cancellationToken">Cancelación de las lecturas de la petición.</param>
    /// <returns>Respuesta HTTP serializable; Query conserva el nombre público por compatibilidad de consumidores.</returns>
    [HttpPost("query")]
    [ProducesResponseType(typeof(AppResponses<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AppResponses<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AppResponses<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(AppResponses<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(AppResponses<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(AppResponses<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(AppResponses<object>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(AppResponses<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AppResponses<object>>> Query(
        [FromBody] ListaDocumentosRadicadosTreeQueryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var requestId = HttpContext.TraceIdentifier;
        var actorResuelto = 0;
        try
        {
            var alias = claimValidationService.ValidateClaim<string>("defaulalias");
            var user = claimValidationService.ValidateClaim<string>("usuarioid");
            var login = claimValidationService.ValidateClaim<string>("login");
            if (alias?.Success != true || string.IsNullOrWhiteSpace(alias.ClaimValue) ||
                user?.Success != true || !int.TryParse(user.ClaimValue, out var usuarioId) || usuarioId <= 0 ||
                login?.Success != true || string.IsNullOrWhiteSpace(login.ClaimValue))
                return StatusCode(StatusCodes.Status403Forbidden,
                    OperationalDocumentListErrorResponseFactory.Create(
                        OperationalDocumentListErrorCatalog.Forbidden, requestId).Response);

            actorResuelto = usuarioId;
            string? workflowAlias = null;
            int? usuarioRadicadorId = null;
            if (string.Equals(request?.ResolutionSource?.Trim(), "RADICACION", StringComparison.OrdinalIgnoreCase))
            {
                var radicador = claimValidationService.ValidateClaim<string>("IdUsuarioRadicador");
                if (radicador?.Success != true || !int.TryParse(radicador.ClaimValue, out var parsed) || parsed <= 0)
                    return StatusCode(StatusCodes.Status403Forbidden,
                        OperationalDocumentListErrorResponseFactory.Create(
                            OperationalDocumentListErrorCatalog.Forbidden, requestId).Response);
                usuarioRadicadorId = parsed;
            }
            if (string.Equals(request?.ResolutionSource?.Trim(), "WORKFLOW", StringComparison.OrdinalIgnoreCase))
            {
                var workflow = claimValidationService.ValidateClaim<string>("defaulaliaswf");
                if (workflow?.Success == true && !string.IsNullOrWhiteSpace(workflow.ClaimValue))
                    workflowAlias = workflow.ClaimValue.Trim();
            }

            var ipAddress = ipHelper.ObtenerDireccionIP(HttpContext);
            if (ipAddress.StartsWith("#", StringComparison.Ordinal)) ipAddress = string.Empty;
            var result = await service.QueryAsync(
                request, usuarioId, alias.ClaimValue.Trim(), workflowAlias,
                login.ClaimValue.Trim(), usuarioRadicadorId, requestId, ipAddress, cancellationToken);
            return StatusCode(result.HttpStatus, result.Response);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception causa)
        {
            var fallo = OperationalDocumentListErrorResponseFactory.Create(
                OperationalDocumentListErrorCatalog.Unexpected, requestId);
            DiagnosticoListadoDocumental.CompletarFallo(fallo.Response, requestId, "boundary", causa);
            if (logger is not null)
                DiagnosticoListadoDocumental.RegistrarFallo(logger, fallo.Response, actorResuelto, 0,
                    OperationalDocumentListErrorCatalog.Unexpected);
            return StatusCode(fallo.HttpStatus, fallo.Response);
        }
    }

}
