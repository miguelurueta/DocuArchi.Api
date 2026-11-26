using System.Security;
using MiApp.DTOs.DTOs.GestorDocumental.Documentos.ListaDocumentosRadicados;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.Documentos.ListaDocumentosRadicados;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.GestorDocumental.Documentos
{
    [Route("api/GestorDocumental/Documentos/ListaDocumentosRadicados")]
    [ApiController]
    [Authorize]
    public sealed class ListaDocumentosRadicadoController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IListaDocumentosRadicadoService _service;
        private readonly ILogger<ListaDocumentosRadicadoController>? _logger;

        /// <summary>Conserva la frontera histórica y registra únicamente diagnóstico seguro de sus fallos.</summary>
        /// <param name="claimValidationService">Validador de identidad autenticada.</param>
        /// <param name="service">Servicio del listado y sus acciones históricas.</param>
        /// <param name="logger">Logger opcional para compatibilidad de construcción; no recibe excepciones crudas.</param>
        public ListaDocumentosRadicadoController(
            IClaimValidationService claimValidationService,
            IListaDocumentosRadicadoService service,
            ILogger<ListaDocumentosRadicadoController>? logger = null)
        {
            _claimValidationService = claimValidationService;
            _service = service;
            _logger = logger;
        }

        /// <summary>Atiende la consulta compatible y conserva diagnóstico seguro sin cambiar sus HTTP históricos.</summary>
        /// <param name="request">Contexto y filtros documentales recibidos; usuario/alias se acreditan desde JWT.</param>
        /// <param name="cancellationToken">Cancelación de la lectura HTTP.</param>
        /// <returns>HTTP 200 o BadRequest históricos con envoltorio compatible y correlación; Query conserva su nombre público.</returns>
        [HttpPost("query")]
        public async Task<ActionResult<AppResponses<object>>> Query(
            [FromBody] ListaDocumentosRadicadosTreeQueryRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var aliasValidation = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!aliasValidation.Success || string.IsNullOrWhiteSpace(aliasValidation.ClaimValue))
                {
                    return BadRequest(aliasValidation.Response);
                }

                var userValidation = _claimValidationService.ValidateClaim<string>("usuarioid");
                if (!userValidation.Success || string.IsNullOrWhiteSpace(userValidation.ClaimValue))
                {
                    return BadRequest(userValidation.Response);
                }

                if (!int.TryParse(userValidation.ClaimValue, out var usuarioId) || usuarioId <= 0)
                {
                    throw new SecurityException("Claim invalido: usuarioid");
                }

                string? workflowAlias = null;
                int? usuarioRadicadorId = null;
                if (string.Equals(request?.ResolutionSource?.Trim(), "RADICACION", StringComparison.OrdinalIgnoreCase))
                {
                    var radicadorValidation = _claimValidationService.ValidateClaim<string>("IdUsuarioRadicador");
                    if (radicadorValidation?.Success != true ||
                        !int.TryParse(radicadorValidation.ClaimValue, out var parsed) || parsed <= 0)
                        return BadRequest(radicadorValidation?.Response);
                    usuarioRadicadorId = parsed;
                }
                if (string.Equals(request?.ResolutionSource?.Trim(), "WORKFLOW", StringComparison.OrdinalIgnoreCase))
                {
                    var workflowValidation = _claimValidationService.ValidateClaim<string>("defaulaliaswf");
                    if (workflowValidation.Success && !string.IsNullOrWhiteSpace(workflowValidation.ClaimValue))
                        workflowAlias = workflowValidation.ClaimValue.Trim();
                }

                var result = await _service.SolicitaListaDocumentosRadicadosTreeAsync(
                    request, usuarioId, aliasValidation.ClaimValue, workflowAlias,
                    HttpContext.TraceIdentifier, cancellationToken,
                    User.FindFirst("login")?.Value?.Trim(), usuarioRadicadorId);
                if (!result.success)
                {
                    result.meta ??= new AppMeta { Status = "error" };
                    result.meta.RequestId = HttpContext.TraceIdentifier;
                    if (result.meta.Metadata is null)
                        DiagnosticoListadoDocumental.CompletarFallo(result, HttpContext.TraceIdentifier,
                            result.meta.Status == "validation" ? "validate" : "query");
                    return BadRequest(result);
                }
               return Ok(result);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception exception)
            {
                var respuesta = new AppResponses<object>
                {
                    success = false,
                    message = "Error en query de ListaDocumentosRadicados",
                    data = null!,
                    meta = new AppMeta { Status = "error", RequestId = HttpContext.TraceIdentifier },
                    errors =
                    [
                        new MiApp.DTOs.DTOs.Errors.AppError
                        {
                            Type = "Exception",
                            Code = "GD_LISTA_DOCUMENTOS_QUERY_UNEXPECTED",
                            Field = "query",
                            Message = "Error en query de ListaDocumentosRadicados",
                            UserMessage = "No fue posible consultar la lista de documentos"
                        }
                    ]
                };
                DiagnosticoListadoDocumental.CompletarFallo(respuesta, HttpContext.TraceIdentifier, "boundary", exception);
                if (_logger is not null)
                    DiagnosticoListadoDocumental.RegistrarFallo(_logger, respuesta, 0, 0, "GD_LISTA_DOCUMENTOS_QUERY_UNEXPECTED");
                return BadRequest(respuesta);
            }
        }

        /// <summary>Atiende acciones históricas sin asumir la ejecución de los módulos del nuevo menú.</summary>
        /// <param name="request">Acción y referencias mínimas recibidas.</param>
        /// <returns>Resultado o error BadRequest compatible; Action mantiene el nombre utilizado por sus consumidores.</returns>
        [HttpPost("action")]
        public async Task<ActionResult<AppResponses<ListaDocumentosRadicadosTreeMutationResultDto?>>> Action([FromBody] ListaDocumentosRadicadosTreeActionRequestDto request)
        {
            try
            {
                var aliasValidation = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!aliasValidation.Success || string.IsNullOrWhiteSpace(aliasValidation.ClaimValue))
                {
                    return BadRequest(aliasValidation.Response);
                }

                var userValidation = _claimValidationService.ValidateClaim<string>("usuarioid");
                if (!userValidation.Success || string.IsNullOrWhiteSpace(userValidation.ClaimValue))
                {
                    return BadRequest(userValidation.Response);
                }

                if (!int.TryParse(userValidation.ClaimValue, out var usuarioId) || usuarioId <= 0)
                {
                    throw new SecurityException("Claim invalido: usuarioid");
                }

                var result = await _service.EjecutaAccionListaDocumentosRadicadosTreeAsync(request, usuarioId, aliasValidation.ClaimValue);
                if (!result.success)
                {
                    result.meta ??= new AppMeta { Status = "error" };
                    result.meta.RequestId = HttpContext.TraceIdentifier;
                    if (result.meta.Metadata is null)
                        DiagnosticoListadoDocumental.CompletarFallo(result, HttpContext.TraceIdentifier,
                            result.meta.Status == "validation" ? "validate" : "query");
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception exception)
            {
                var respuesta = new AppResponses<ListaDocumentosRadicadosTreeMutationResultDto?>
                {
                    success = false,
                    message = "Error en action de ListaDocumentosRadicados",
                    data = null,
                    meta = new AppMeta { Status = "error", RequestId = HttpContext.TraceIdentifier },
                    errors =
                    [
                        new MiApp.DTOs.DTOs.Errors.AppError
                        {
                            Type = "Exception",
                            Code = "GD_LISTA_DOCUMENTOS_ACTION_UNEXPECTED",
                            Field = "action",
                            Message = "Error en action de ListaDocumentosRadicados",
                            UserMessage = "No fue posible procesar la accion solicitada"
                        }
                    ]
                };
                DiagnosticoListadoDocumental.CompletarFallo(respuesta, HttpContext.TraceIdentifier, "action", exception);
                if (_logger is not null)
                    DiagnosticoListadoDocumental.RegistrarFallo(_logger, respuesta, 0, 0, "GD_LISTA_DOCUMENTOS_ACTION_UNEXPECTED");
                return BadRequest(respuesta);
            }
        }
    }
}
