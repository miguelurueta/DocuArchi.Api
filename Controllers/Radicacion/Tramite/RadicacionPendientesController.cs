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

        public RadicacionPendientesController(
            IClaimValidationService claimValidationService,
            IRadicacionPendienteEstadoActivoService estadoActivoService,
            IRadicacionPendientesContadorService contadorService)
        {
            _claimValidationService = claimValidationService;
            _estadoActivoService = estadoActivoService;
            _contadorService = contadorService;
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
    }
}
