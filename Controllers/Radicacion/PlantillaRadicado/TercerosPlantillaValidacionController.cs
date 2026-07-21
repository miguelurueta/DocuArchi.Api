using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.Radicacion.PlantillaValidacion;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.Radicacion.PlantillaValidacion;
using MiApp.Services.Service.Radicacion.PlantillaValidacion.Errors;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.Radicacion.PlantillaRadicado
{
    [ApiController]
    [Authorize]
    [Route("api/PlantillaRadicado/terceros")]
    public sealed class TercerosPlantillaValidacionController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly ITercerosPlantillaValidacionService _service;
        private readonly ILogger<TercerosPlantillaValidacionController> _logger;

        public TercerosPlantillaValidacionController(
            IClaimValidationService claimValidationService,
            ITercerosPlantillaValidacionService service,
            ILogger<TercerosPlantillaValidacionController> logger)
        {
            _claimValidationService = claimValidationService;
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene la estructura dinamica del formulario de tercero/remitente para crear o editar registros.
        /// </summary>
        /// <param name="request">IdScript, modo de uso e IdRegistro cuando el modo es editar.</param>
        /// <returns>Contrato AppResponses con metadata de campos, valores actuales y permisos de UI.</returns>
        [HttpPost("formulario")]
        [ProducesResponseType(typeof(AppResponses<TerceroFormularioResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<TerceroFormularioResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponses<TerceroFormularioResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AppResponses<TerceroFormularioResponseDto>>> ObtenerFormulario(
            [FromBody] TerceroFormularioRequestDto request)
        {
            var requestId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");

            try
            {
                var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!validation.Success || string.IsNullOrWhiteSpace(validation.ClaimValue))
                {
                    var claimResponse = TercerosPlantillaValidacionErrorResponseFactory.Build<TerceroFormularioResponseDto>(
                        TercerosPlantillaValidacionErrorCatalog.ClaimRequired,
                        requestId,
                        "defaulalias");

                    return BadRequest(claimResponse);
                }

                var result = await _service.ObtenerFormularioAsync(
                    request,
                    validation.ClaimValue,
                    requestId);

                if (!result.success)
                {
                    return StatusCode(ResolveStatus(result), result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "terceros_formulario_controller_unexpected requestId={RequestId}", requestId);

                var response = TercerosPlantillaValidacionErrorResponseFactory.Build<TerceroFormularioResponseDto>(
                    TercerosPlantillaValidacionErrorCatalog.Unexpected,
                    requestId,
                    nameof(ObtenerFormulario),
                    ex.Message);

                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        private static int ResolveStatus(AppResponses<TerceroFormularioResponseDto> result)
        {
            var code = result.errors?
                .OfType<AppError>()
                .FirstOrDefault()?
                .Code;

            return TercerosPlantillaValidacionErrorResponseFactory.ResolveHttpStatus(code);
        }
    }
}
