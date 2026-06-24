using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.Indexacion;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.Indexacion;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.GestionCorrespondencia.GestionRespuesta.AgentContext
{
    [Authorize]
    [ApiController]
    [Route("api/GestionCorrespondencia/respuestas/contexto-historico/indexacion")]
    public sealed class RespuestaHistoricalContextIndexacionController : ControllerBase
    {
        private static readonly HashSet<string> TechnicalPermissions = new(StringComparer.OrdinalIgnoreCase)
        {
            "ctx-hist-index-admin",
            "contexto-historico-indexacion-admin",
            "gestion-correspondencia-contexto-historico-indexacion",
            "admin"
        };

        private readonly IClaimValidationService _claimValidationService;
        private readonly IRespuestaHistoricalContextIndexacionAdminService _service;
        private readonly ILogger<RespuestaHistoricalContextIndexacionController> _logger;

        public RespuestaHistoricalContextIndexacionController(
            IClaimValidationService claimValidationService,
            IRespuestaHistoricalContextIndexacionAdminService service,
            ILogger<RespuestaHistoricalContextIndexacionController> logger)
        {
            _claimValidationService = claimValidationService;
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Inicia o reanuda backfill administrativo del contexto historico.
        /// </summary>
        [HttpPost("backfill")]
        [ProducesResponseType(typeof(AppResponses<RespuestaHistoricalIndexacionOperationResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<RespuestaHistoricalIndexacionOperationResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponses<RespuestaHistoricalIndexacionOperationResponseDto>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(AppResponses<RespuestaHistoricalIndexacionOperationResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AppResponses<RespuestaHistoricalIndexacionOperationResponseDto>>> Backfill(
            [FromBody] RespuestaHistoricalBackfillRequestDto request)
        {
            var context = ValidateContext<RespuestaHistoricalIndexacionOperationResponseDto>();
            if (!context.Response.success)
            {
                return MapFailure(context.Response);
            }

            var result = await _service.StartBackfillAsync(request, context.Context);
            LogOperation("backfill", context.Context, result);
            return MapOperationResult(result);
        }

        /// <summary>
        /// Reindexa una respuesta especifica de forma auditable.
        /// </summary>
        [HttpPost("reindexar")]
        [ProducesResponseType(typeof(AppResponses<RespuestaHistoricalIndexacionOperationResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<RespuestaHistoricalIndexacionOperationResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponses<RespuestaHistoricalIndexacionOperationResponseDto>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(AppResponses<RespuestaHistoricalIndexacionOperationResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AppResponses<RespuestaHistoricalIndexacionOperationResponseDto>>> Reindexar(
            [FromBody] RespuestaHistoricalReindexRequestDto request)
        {
            var context = ValidateContext<RespuestaHistoricalIndexacionOperationResponseDto>();
            if (!context.Response.success)
            {
                return MapFailure(context.Response);
            }

            var result = await _service.ReindexAsync(request, context.Context);
            LogOperation("reindexar", context.Context, result);
            return MapOperationResult(result);
        }

        /// <summary>
        /// Reconcila una ventana temporal o rango de respuestas del contexto historico.
        /// </summary>
        [HttpPost("reconciliar")]
        [ProducesResponseType(typeof(AppResponses<RespuestaHistoricalIndexacionOperationResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<RespuestaHistoricalIndexacionOperationResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponses<RespuestaHistoricalIndexacionOperationResponseDto>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(AppResponses<RespuestaHistoricalIndexacionOperationResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AppResponses<RespuestaHistoricalIndexacionOperationResponseDto>>> Reconciliar(
            [FromBody] RespuestaHistoricalReconcileRequestDto request)
        {
            var context = ValidateContext<RespuestaHistoricalIndexacionOperationResponseDto>();
            if (!context.Response.success)
            {
                return MapFailure(context.Response);
            }

            var result = await _service.ReconcileAsync(request, context.Context);
            LogOperation("reconciliar", context.Context, result);
            return MapOperationResult(result);
        }

        /// <summary>
        /// Consulta estado operativo sanitizado de indexacion.
        /// </summary>
        [HttpGet("estado")]
        [ProducesResponseType(typeof(AppResponses<RespuestaHistoricalIndexacionStatusResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<RespuestaHistoricalIndexacionStatusResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponses<RespuestaHistoricalIndexacionStatusResponseDto>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(AppResponses<RespuestaHistoricalIndexacionStatusResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AppResponses<RespuestaHistoricalIndexacionStatusResponseDto>>> Estado()
        {
            var context = ValidateContext<RespuestaHistoricalIndexacionStatusResponseDto>();
            if (!context.Response.success)
            {
                return MapFailure(context.Response);
            }

            var result = await _service.GetStatusAsync(context.Context);
            _logger.LogInformation(
                "RespuestaHistoricalContextIndexacion estado requestId={RequestId} usuarioId={UsuarioId} alias={Alias} success={Success}",
                context.Context.RequestId,
                context.Context.UsuarioId,
                context.Context.DefaultDbAlias,
                result.success);

            return result.success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Reintenta registros fallidos recuperables del contexto historico.
        /// </summary>
        [HttpPost("reintentar-fallidos")]
        [ProducesResponseType(typeof(AppResponses<RespuestaHistoricalIndexacionOperationResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<RespuestaHistoricalIndexacionOperationResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponses<RespuestaHistoricalIndexacionOperationResponseDto>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(AppResponses<RespuestaHistoricalIndexacionOperationResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AppResponses<RespuestaHistoricalIndexacionOperationResponseDto>>> ReintentarFallidos(
            [FromBody] RespuestaHistoricalRetryFailedRequestDto request)
        {
            var context = ValidateContext<RespuestaHistoricalIndexacionOperationResponseDto>();
            if (!context.Response.success)
            {
                return MapFailure(context.Response);
            }

            var result = await _service.RetryFailedAsync(request, context.Context);
            LogOperation("reintentar-fallidos", context.Context, result);
            return MapOperationResult(result);
        }

        private ContextValidation<T> ValidateContext<T>()
        {
            var requestId = ResolveRequestId();

            var aliasValidation = _claimValidationService.ValidateClaim<T>("defaulalias");
            if (!aliasValidation.Success || string.IsNullOrWhiteSpace(aliasValidation.ClaimValue))
            {
                return new ContextValidation<T>(
                    new RespuestaHistoricalIndexacionAdminContext { RequestId = requestId },
                    aliasValidation.Response ?? Validation<T>("defaulalias", "Claim defaulalias requerido", requestId));
            }

            var usuarioValidation = _claimValidationService.ValidateClaim<T>("usuarioid");
            if (!usuarioValidation.Success || string.IsNullOrWhiteSpace(usuarioValidation.ClaimValue))
            {
                return new ContextValidation<T>(
                    new RespuestaHistoricalIndexacionAdminContext { RequestId = requestId },
                    usuarioValidation.Response ?? Validation<T>("usuarioid", "Claim usuarioid requerido", requestId));
            }

            if (!int.TryParse(usuarioValidation.ClaimValue, out var usuarioId) || usuarioId <= 0)
            {
                return new ContextValidation<T>(
                    new RespuestaHistoricalIndexacionAdminContext { RequestId = requestId },
                    Validation<T>("usuarioid", "Claim usuarioid invalido", requestId));
            }

            if (!HasTechnicalPermission())
            {
                return new ContextValidation<T>(
                    new RespuestaHistoricalIndexacionAdminContext { RequestId = requestId },
                    Forbidden<T>(requestId));
            }

            return new ContextValidation<T>(
                new RespuestaHistoricalIndexacionAdminContext
                {
                    RequestId = requestId,
                    DefaultDbAlias = aliasValidation.ClaimValue,
                    UsuarioId = usuarioId,
                    UserName = User.Identity?.Name
                },
                Success<T>(requestId));
        }

        private bool HasTechnicalPermission()
        {
            if (User.IsInRole("admin") || User.IsInRole("ctx-hist-index-admin"))
            {
                return true;
            }

            return User.Claims.Any(claim =>
                (claim.Type.Equals("permission", StringComparison.OrdinalIgnoreCase)
                    || claim.Type.Equals("permissions", StringComparison.OrdinalIgnoreCase)
                    || claim.Type.Equals("role", StringComparison.OrdinalIgnoreCase)
                    || claim.Type.Equals("rol", StringComparison.OrdinalIgnoreCase)
                    || claim.Type.Equals("esadmin", StringComparison.OrdinalIgnoreCase)
                    || claim.Type.Equals("isadmin", StringComparison.OrdinalIgnoreCase))
                && TechnicalPermissions.Contains(claim.Value));
        }

        private string ResolveRequestId()
        {
            var header = Request.Headers["X-Request-Id"].ToString();
            return string.IsNullOrWhiteSpace(header) ? HttpContext.TraceIdentifier : header;
        }

        private void LogOperation(
            string endpoint,
            RespuestaHistoricalIndexacionAdminContext context,
            AppResponses<RespuestaHistoricalIndexacionOperationResponseDto> result)
        {
            _logger.LogInformation(
                "RespuestaHistoricalContextIndexacion endpoint={Endpoint} requestId={RequestId} operationId={OperationId} usuarioId={UsuarioId} alias={Alias} mode={Mode} dryRun={DryRun} force={Force} success={Success} status={Status}",
                endpoint,
                context.RequestId,
                result.data?.OperationId,
                context.UsuarioId,
                context.DefaultDbAlias,
                result.data?.Mode,
                result.data?.DryRun,
                result.data?.Force,
                result.success,
                result.meta?.Status);
        }

        private ActionResult<AppResponses<RespuestaHistoricalIndexacionOperationResponseDto>> MapOperationResult(
            AppResponses<RespuestaHistoricalIndexacionOperationResponseDto> result)
        {
            return result.success ? Ok(result) : BadRequest(result);
        }

        private ActionResult<AppResponses<T>> MapFailure<T>(AppResponses<T> result)
        {
            var code = result.errors?.OfType<AppError>().FirstOrDefault()?.Code;
            return code == "CTX_HIST_INDEX_FORBIDDEN" ? StatusCode(StatusCodes.Status403Forbidden, result) : BadRequest(result);
        }

        private static AppResponses<T> Success<T>(string requestId)
        {
            return new AppResponses<T>
            {
                success = true,
                message = "OK",
                meta = new AppMeta { Status = "success", RequestId = requestId },
                errors = []
            };
        }

        private static AppResponses<T> Validation<T>(string field, string message, string requestId)
        {
            return new AppResponses<T>
            {
                success = false,
                message = message,
                meta = new AppMeta { Status = "invalid", RequestId = requestId },
                errors =
                [
                    new AppError
                    {
                        Type = "Validation",
                        Code = "CTX_HIST_INDEX_INVALID_CLAIM",
                        Field = field,
                        Message = message,
                        UserMessage = "No fue posible validar la sesion para operar contexto historico."
                    }
                ]
            };
        }

        private static AppResponses<T> Forbidden<T>(string requestId)
        {
            return new AppResponses<T>
            {
                success = false,
                message = "Permiso tecnico requerido",
                meta = new AppMeta { Status = "forbidden", RequestId = requestId },
                errors =
                [
                    new AppError
                    {
                        Type = "Authorization",
                        Code = "CTX_HIST_INDEX_FORBIDDEN",
                        Field = "permission",
                        Message = "Permiso tecnico requerido",
                        UserMessage = "No tiene permisos para operar la indexacion del contexto historico."
                    }
                ]
            };
        }

        private sealed record ContextValidation<T>(
            RespuestaHistoricalIndexacionAdminContext Context,
            AppResponses<T> Response);
    }
}
