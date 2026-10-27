using System.Diagnostics;
using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestionCorrespondencia.GestionRespuesta.AgentContext.ContextoHistorico.Consulta;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.GestionCorrespondencia.GestionRespuesta.AgentContext
{
    [Authorize]
    [ApiController]
    [Route("api/GestionCorrespondencia/respuestas/contexto-historico")]
    public sealed class RespuestaHistoricalContextController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IRespuestaHistoricalContextService _service;
        private readonly ILogger<RespuestaHistoricalContextController> _logger;

        public RespuestaHistoricalContextController(
            IClaimValidationService claimValidationService,
            IRespuestaHistoricalContextService service,
            ILogger<RespuestaHistoricalContextController> logger)
        {
            _claimValidationService = claimValidationService;
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Consulta contexto historico preparado para uso por tool calling.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(AppResponses<RespuestaHistoricalContextResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<RespuestaHistoricalContextResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponses<RespuestaHistoricalContextResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AppResponses<RespuestaHistoricalContextResponseDto>>> GetContext(
            [FromBody] RespuestaHistoricalContextRequestDto request)
        {
            var sw = Stopwatch.StartNew();
            var requestId = ResolveRequestId();

            var aliasValidation = _claimValidationService.ValidateClaim<RespuestaHistoricalContextResponseDto>("defaulalias");
            if (!aliasValidation.Success || string.IsNullOrWhiteSpace(aliasValidation.ClaimValue))
            {
                return BadRequest(aliasValidation.Response);
            }

            var usuarioValidation = _claimValidationService.ValidateClaim<RespuestaHistoricalContextResponseDto>("usuarioid");
            if (!usuarioValidation.Success || string.IsNullOrWhiteSpace(usuarioValidation.ClaimValue))
            {
                return BadRequest(usuarioValidation.Response);
            }

            if (!int.TryParse(usuarioValidation.ClaimValue, out var usuarioId) || usuarioId <= 0)
            {
                return BadRequest(Validation("usuarioid", "Claim usuarioid invalido", requestId));
            }

            _logger.LogInformation("Context requestId={RequestId} usuarioId={UsuarioId} stage={Stage} operation={Operation}",
                requestId, usuarioId, "http", "read-context");

            var result = await _service.GetContextAsync(request, aliasValidation.ClaimValue, usuarioId, requestId);
            sw.Stop();

            _logger.LogInformation(
                "Context completed requestId={RequestId} usuarioId={UsuarioId} metaStatus={MetaStatus} success={Success} durationMs={DurationMs}",
                requestId,
                usuarioId,
                result.meta?.Status,
                result.success,
                sw.ElapsedMilliseconds);

            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        private string ResolveRequestId()
        {
            var header = Request.Headers["X-Request-Id"].ToString();
            return string.IsNullOrWhiteSpace(header) || header.Length > 120 || header.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not ('-' or '_' or ':' or '.')) ? HttpContext.TraceIdentifier : header;
        }

        private static AppResponses<RespuestaHistoricalContextResponseDto> Validation(
            string field,
            string message,
            string requestId)
        {
            return new AppResponses<RespuestaHistoricalContextResponseDto>
            {
                success = false,
                message = message,
                data = new RespuestaHistoricalContextResponseDto
                {
                    RequestId = requestId,
                    ContextStatus = "failed"
                },
                meta = new AppMeta { Status = "invalid", RequestId = requestId },
                errors =
                [
                    new AppError
                    {
                        Type = "Validation",
                        Code = "CTX_HIST_INVALID_CLAIM",
                        Field = field,
                        Message = message,
                        UserMessage = "No fue posible validar la sesion para consultar contexto historico."
                    }
                ]
            };
        }
    }
}
