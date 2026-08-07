using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.Radicacion.Tramite;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.Radicacion.Tramite;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.Radicacion.Tramite
{
    [Route("api/radicacion")]
    [ApiController]
    [Authorize]
    public sealed class RadicacionPendientesController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IRadicacionPendienteEstadoActivoService _estadoActivoService;
        private readonly IRadicacionPendientesContadorService _contadorService;
        private readonly IEnviarRadicadoPendienteService _enviarPendienteService;
        private readonly ITomarRadicadoPendienteService _tomarPendienteService;

        public RadicacionPendientesController(
            IClaimValidationService claimValidationService,
            IRadicacionPendienteEstadoActivoService estadoActivoService,
            IRadicacionPendientesContadorService contadorService,
            IEnviarRadicadoPendienteService enviarPendienteService,
            ITomarRadicadoPendienteService tomarPendienteService)
        {
            _claimValidationService = claimValidationService;
            _estadoActivoService = estadoActivoService;
            _contadorService = contadorService;
            _enviarPendienteService = enviarPendienteService;
            _tomarPendienteService = tomarPendienteService;
        }

        /// <summary>
        /// Consulta si el usuario autenticado tiene un tramite de radicacion activo en estado 0.
        /// </summary>
        /// <returns>Contrato AppResponses con el contexto minimo de reentrada documental.</returns>
        [HttpGet("pendientes/estado-activo")]
        [ProducesResponseType(typeof(AppResponses<RadicacionPendienteEstadoActivoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<RadicacionPendienteEstadoActivoDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponses<RadicacionPendienteEstadoActivoDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AppResponses<RadicacionPendienteEstadoActivoDto>>> EstadoActivo()
        {
            var requestId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");
            try
            {
                var validationAlias = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!validationAlias.Success || string.IsNullOrWhiteSpace(validationAlias.ClaimValue))
                {
                    return BadRequest(validationAlias.Response);
                }

                var validationUsuario = _claimValidationService.ValidateClaim<string>("usuarioid");
                if (!validationUsuario.Success || string.IsNullOrWhiteSpace(validationUsuario.ClaimValue))
                {
                    return BadRequest(validationUsuario.Response);
                }

                var validationAliasWorkflow = _claimValidationService.ValidateClaim<string>("defaulaliaswf");
                var workflowAlias = validationAliasWorkflow.Success ? validationAliasWorkflow.ClaimValue : null;

                if (!int.TryParse(validationUsuario.ClaimValue, out var idUsuarioGestion))
                {
                    return BadRequest(BuildError(
                        "RADICACION_ACTIVO_CLAIM_INVALID",
                        "Claim usuarioid invalido",
                        "No fue posible identificar el usuario autenticado.",
                        requestId,
                        "usuarioid"));
                }

                var result = await _estadoActivoService.SolicitaEstadoActivo(
                    idUsuarioGestion,
                    validationAlias.ClaimValue,
                    requestId,
                    workflowAlias);
                if (!result.success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    BuildError(
                        "RADICACION_ACTIVO_UNEXPECTED",
                        "Error inesperado al consultar estado activo de radicacion",
                        "No fue posible consultar si existe un tramite activo de radicacion.",
                        requestId));
            }
        }

        /// <summary>
        /// Consulta el total liviano de radicados pendientes estado 1 para el usuario autenticado.
        /// </summary>
        /// <returns>Contrato AppResponses con el total de pendientes sin cargar filas del listado.</returns>
        [HttpGet("pendientes/contador")]
        [ProducesResponseType(typeof(AppResponses<RadicacionPendientesContadorDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<RadicacionPendientesContadorDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponses<RadicacionPendientesContadorDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AppResponses<RadicacionPendientesContadorDto>>> Contador()
        {
            var requestId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");
            try
            {
                var validationAlias = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!validationAlias.Success || string.IsNullOrWhiteSpace(validationAlias.ClaimValue))
                {
                    return BadRequest(validationAlias.Response);
                }

                var validationUsuario = _claimValidationService.ValidateClaim<string>("usuarioid");
                if (!validationUsuario.Success || string.IsNullOrWhiteSpace(validationUsuario.ClaimValue))
                {
                    return BadRequest(validationUsuario.Response);
                }

                if (!int.TryParse(validationUsuario.ClaimValue, out var idUsuarioGestion))
                {
                    return BadRequest(BuildContadorError(
                        "RADICACION_CONTADOR_CLAIM_INVALID",
                        "Claim usuarioid invalido",
                        "No fue posible identificar el usuario autenticado.",
                        requestId,
                        "usuarioid"));
                }

                var result = await _contadorService.SolicitaTotalPendientes(
                    idUsuarioGestion,
                    validationAlias.ClaimValue,
                    requestId);
                if (!result.success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    BuildContadorError(
                        "RADICACION_CONTADOR_UNEXPECTED",
                        "Error inesperado al consultar contador de radicados pendientes",
                        "No fue posible consultar el contador de radicados pendientes.",
                        requestId));
            }
        }

        /// <summary>
        /// Envia un radicado activo de gestion documental a pendiente mediante transicion estricta estado 0 a estado 1.
        /// </summary>
        /// <param name="idEstadoRadicado">Identificador del registro de estado de radicacion.</param>
        /// <param name="request">Motivo opcional de la operacion.</param>
        /// <returns>Contrato AppResponses con el estado anterior, estado actual y destino post registro.</returns>
        [HttpPost("pendientes/{idEstadoRadicado:long}/enviar-pendiente")]
        [ProducesResponseType(typeof(AppResponses<EnviarRadicadoPendienteResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<EnviarRadicadoPendienteResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponses<EnviarRadicadoPendienteResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AppResponses<EnviarRadicadoPendienteResponseDto>>> EnviarPendiente(
            long idEstadoRadicado,
            [FromBody] EnviarRadicadoPendienteRequestDto? request)
        {
            var requestId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");
            try
            {
                var validationAlias = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!validationAlias.Success || string.IsNullOrWhiteSpace(validationAlias.ClaimValue))
                {
                    return BadRequest(validationAlias.Response);
                }

                var validationUsuario = _claimValidationService.ValidateClaim<string>("usuarioid");
                if (!validationUsuario.Success || string.IsNullOrWhiteSpace(validationUsuario.ClaimValue))
                {
                    return BadRequest(validationUsuario.Response);
                }

                if (!int.TryParse(validationUsuario.ClaimValue, out var idUsuarioGestion))
                {
                    return BadRequest(BuildEnviarPendienteError(
                        "RADICACION_ENVIAR_PENDIENTE_CLAIM_INVALID",
                        "Claim usuarioid invalido",
                        "No fue posible identificar el usuario autenticado.",
                        requestId,
                        "usuarioid"));
                }

                var result = await _enviarPendienteService.EnviaPendiente(
                    idEstadoRadicado,
                    idUsuarioGestion,
                    validationAlias.ClaimValue,
                    request,
                    requestId);
                if (!result.success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    BuildEnviarPendienteError(
                        "RADICACION_ENVIAR_PENDIENTE_UNEXPECTED",
                        "Error inesperado al enviar radicado a pendiente",
                        "No fue posible enviar el radicado a pendiente.",
                        requestId));
            }
        }

        /// <summary>
        /// Toma un radicado pendiente para gestion documental mediante transicion estricta estado 1 a estado 0.
        /// </summary>
        /// <param name="idEstadoRadicado">Identificador del registro de estado de radicacion pendiente.</param>
        /// <param name="request">Tarea workflow opcional cuando la fila del listado ya la tiene disponible.</param>
        /// <returns>Contrato AppResponses con contexto documental compatible con estado-activo.</returns>
        [HttpPost("pendientes/{idEstadoRadicado:long}/tomar")]
        [ProducesResponseType(typeof(AppResponses<TomarRadicadoPendienteResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<TomarRadicadoPendienteResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponses<TomarRadicadoPendienteResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AppResponses<TomarRadicadoPendienteResponseDto>>> TomarPendiente(
            long idEstadoRadicado,
            [FromBody] TomarRadicadoPendienteRequestDto? request)
        {
            var requestId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");
            try
            {
                var validationAlias = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!validationAlias.Success || string.IsNullOrWhiteSpace(validationAlias.ClaimValue))
                {
                    return BadRequest(validationAlias.Response);
                }

                var validationUsuario = _claimValidationService.ValidateClaim<string>("usuarioid");
                if (!validationUsuario.Success || string.IsNullOrWhiteSpace(validationUsuario.ClaimValue))
                {
                    return BadRequest(validationUsuario.Response);
                }

                if (!int.TryParse(validationUsuario.ClaimValue, out var idUsuarioGestion))
                {
                    return BadRequest(BuildTomarPendienteError(
                        "RADICACION_TOMAR_PENDIENTE_CLAIM_INVALID",
                        "Claim usuarioid invalido",
                        "No fue posible identificar el usuario autenticado.",
                        requestId,
                        "usuarioid"));
                }

                var validationAliasWorkflow = _claimValidationService.ValidateClaim<string>("defaulaliaswf");
                var defaultAliasWorkflow = validationAliasWorkflow.Success
                    ? validationAliasWorkflow.ClaimValue
                    : null;

                var result = await _tomarPendienteService.TomaPendiente(
                    idEstadoRadicado,
                    idUsuarioGestion,
                    validationAlias.ClaimValue,
                    defaultAliasWorkflow,
                    request,
                    requestId);
                if (!result.success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    BuildTomarPendienteError(
                        "RADICACION_TOMAR_PENDIENTE_UNEXPECTED",
                        "Error inesperado al tomar radicado pendiente",
                        "No fue posible tomar el radicado pendiente.",
                        requestId));
            }
        }

        private static AppResponses<RadicacionPendienteEstadoActivoDto> BuildError(
            string code,
            string message,
            string userMessage,
            string requestId,
            string? field = null)
        {
            return new AppResponses<RadicacionPendienteEstadoActivoDto>
            {
                success = false,
                message = $"{message}. Code={code}; requestId={requestId}",
                data = null!,
                errors =
                [
                    new AppError
                    {
                        Type = "Validation",
                        Code = code,
                        Message = message,
                        UserMessage = userMessage,
                        Field = field
                    }
                ],
                meta = new AppMeta
                {
                    Status = "ERROR",
                    RequestId = requestId,
                    ResponseVersion = "1.0"
                }
            };
        }

        private static AppResponses<RadicacionPendientesContadorDto> BuildContadorError(
            string code,
            string message,
            string userMessage,
            string requestId,
            string? field = null)
        {
            return new AppResponses<RadicacionPendientesContadorDto>
            {
                success = false,
                message = $"{message}. Code={code}; requestId={requestId}",
                data = null!,
                errors =
                [
                    new AppError
                    {
                        Type = "Validation",
                        Code = code,
                        Message = message,
                        UserMessage = userMessage,
                        Field = field
                    }
                ],
                meta = new AppMeta
                {
                    Status = "ERROR",
                    RequestId = requestId,
                    ResponseVersion = "1.0"
                }
            };
        }

        private static AppResponses<EnviarRadicadoPendienteResponseDto> BuildEnviarPendienteError(
            string code,
            string message,
            string userMessage,
            string requestId,
            string? field = null)
        {
            return new AppResponses<EnviarRadicadoPendienteResponseDto>
            {
                success = false,
                message = $"{message}. Code={code}; requestId={requestId}",
                data = null!,
                errors =
                [
                    new AppError
                    {
                        Type = "Validation",
                        Code = code,
                        Message = message,
                        UserMessage = userMessage,
                        Field = field
                    }
                ],
                meta = new AppMeta
                {
                    Status = "ERROR",
                    RequestId = requestId,
                    ResponseVersion = "1.0"
                }
            };
        }

        private static AppResponses<TomarRadicadoPendienteResponseDto> BuildTomarPendienteError(
            string code,
            string message,
            string userMessage,
            string requestId,
            string? field = null)
        {
            return new AppResponses<TomarRadicadoPendienteResponseDto>
            {
                success = false,
                message = $"{message}. Code={code}; requestId={requestId}",
                data = null!,
                errors =
                [
                    new AppError
                    {
                        Type = "Validation",
                        Code = code,
                        Message = message,
                        UserMessage = userMessage,
                        Field = field
                    }
                ],
                meta = new AppMeta
                {
                    Status = "ERROR",
                    RequestId = requestId,
                    ResponseVersion = "1.0"
                }
            };
        }
    }
}
