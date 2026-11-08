using MiApp.DTOs.DTOs.Radicacion.Tramite;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.Radicacion.Tramite;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using MiApp.Services.Service.SessionHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security;
using MiApp.Services.Service.Radicacion.Tramite.Errors;
using Microsoft.Extensions.Logging.Abstractions;

namespace DocuArchi.Api.Controllers.Radicacion.Tramite
{
    [Route("api/radicacion")]
    [ApiController]
    [Authorize]
    public sealed class RadicacionController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IRegistrarRadicacionEntranteService _registrarService;
        private readonly IValidarRadicacionEntranteService _validarService;
        private readonly IFlujoInicialRadicacionService _flujoInicialService;
        private readonly IIpHelper _ipHelper;
        private readonly ILogger<RadicacionController> _logger;

        /// <summary>Compone los casos de uso existentes de Radicación y el registro seguro de fallos de la frontera HTTP.</summary>
        /// <param name="claimValidationService">Valida los claims de la sesión autenticada.</param>
        /// <param name="registrarService">Ejecuta el alta y conserva sus resultados parciales.</param>
        /// <param name="validarService">Valida la solicitud sin registrar.</param>
        /// <param name="flujoInicialService">Consulta el flujo inicial configurado.</param>
        /// <param name="ipHelper">Obtiene el origen para la auditoría de dominio.</param>
        /// <param name="logger">Registra códigos y correlación sin causas crudas; opcional para consumidores existentes.</param>
        public RadicacionController(
            IClaimValidationService claimValidationService,
            IRegistrarRadicacionEntranteService registrarService,
            IValidarRadicacionEntranteService validarService,
            IFlujoInicialRadicacionService flujoInicialService,
            IIpHelper ipHelper,
            ILogger<RadicacionController>? logger = null)
        {
            _claimValidationService = claimValidationService;
            _registrarService = registrarService;
            _validarService = validarService;
            _flujoInicialService = flujoInicialService;
            _ipHelper = ipHelper;
            _logger = logger ?? NullLogger<RadicacionController>.Instance;
        }

        /// <summary>
        /// Registra una radicacion entrante validando claims de sesion y devolviendo AppResponses estandarizado.
        /// </summary>
        /// <param name="request">Payload de radicacion entrante recibido desde frontend.</param>
        /// <param name="tipoModuloRadicacion">Tipo de modulo de radicacion enviado por query string. El valor por defecto es 1.</param>
        /// <returns>Resultado del registro con estado, mensaje, data y errores.</returns>
        [HttpPost("registrar-entrante")]
        public async Task<ActionResult<AppResponses<RegistrarRadicacionEntranteResponseDto>>> RegistrarEntrante(
            [FromBody] RegistrarRadicacionEntranteRequestDto request,
            [FromQuery] int tipoModuloRadicacion = 1)
        {
            var duracion = System.Diagnostics.Stopwatch.StartNew();
            int? usuarioGestionResuelto = null;
            try
            {
                var aliasValidation = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!aliasValidation.Success || aliasValidation.ClaimValue == null)
                {
                    return BadRequest(aliasValidation.Response);
                }

                var userValidation = _claimValidationService.ValidateClaim<string>("usuarioid");
                if (!userValidation.Success || userValidation.ClaimValue == null)
                {
                    return BadRequest(userValidation.Response);
                }

                if (!int.TryParse(userValidation.ClaimValue, out var idUsuarioGestion))
                {
                    throw new SecurityException("Claim invalido: usuarioid");
                }
                usuarioGestionResuelto = idUsuarioGestion;

                var result = await _registrarService.RegistrarRadicacionEntranteAsync(
                    request,
                    idUsuarioGestion,
                    aliasValidation.ClaimValue,
                    _ipHelper.ObtenerDireccionIP(HttpContext),
                    tipoModuloRadicacion);

                if (!result.success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                var respuesta = ErroresAltaRadicacion.CrearFalloInesperadoRegistro<RegistrarRadicacionEntranteResponseDto>(
                    "Error inesperado al registrar radicacion entrante", HttpContext.TraceIdentifier, ex);
                respuesta.meta!.Metadata!["Stage"] = "http";
                _logger.LogWarning("Radicacion RequestId {RequestId} Stage {Stage} Operation {Operation} ErrorCode {ErrorCode} MetaStatus {MetaStatus} DurationMs {DurationMs} UsuarioId {UsuarioId}",
                    respuesta.meta.RequestId, "http", "registrar-entrante", "RADICACION_REGISTRO_UNEXPECTED", respuesta.meta.Status,
                    duracion.ElapsedMilliseconds, usuarioGestionResuelto);
                return StatusCode(StatusCodes.Status500InternalServerError, respuesta);
            }
        }

        [HttpPost("validar-entrante")]
        public async Task<ActionResult<AppResponses<ValidarRadicacionEntranteResponseDto>>> ValidarEntrante(
            [FromBody] ValidarRadicacionEntranteRequestDto request)
        {
            var result = await _validarService.ValidarRadicacionEntranteAsync(request);
            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("flujo-inicial")]
        public async Task<ActionResult<AppResponses<FlujoInicialDto>>> FlujoInicial([FromQuery] int idTipoTramite)
        {
            var aliasValidation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!aliasValidation.Success || aliasValidation.ClaimValue == null)
            {
                return BadRequest(aliasValidation.Response);
            }

            var result = await _flujoInicialService.ObtenerFlujoInicialAsync(idTipoTramite, aliasValidation.ClaimValue);
            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
