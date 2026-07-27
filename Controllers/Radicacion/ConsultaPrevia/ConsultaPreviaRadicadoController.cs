using MiApp.DTOs.DTOs.Radicacion.ConsultaPrevia;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.Radicacion.ConsultaPrevia.Core;
using MiApp.Services.Service.Radicacion.ConsultaPrevia.Errors;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.Radicacion.ConsultaPrevia
{
    [Route("api/radicacion/consulta-previa")]
    [ApiController]
    [Authorize]
    public sealed class ConsultaPreviaRadicadoController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IConsultaPreviaRadicadoService _consultaPreviaRadicadoService;
        private readonly ConsultaPreviaRadicadoErrorResponseFactory _errorFactory;

        public ConsultaPreviaRadicadoController(
            IClaimValidationService claimValidationService,
            IConsultaPreviaRadicadoService consultaPreviaRadicadoService,
            ConsultaPreviaRadicadoErrorResponseFactory errorFactory)
        {
            _claimValidationService = claimValidationService;
            _consultaPreviaRadicadoService = consultaPreviaRadicadoService;
            _errorFactory = errorFactory;
        }

        /// <summary>
        /// Obtiene la estructura dinamica de campos consultables para Consulta Previa de Radicado.
        /// </summary>
        /// <param name="idPlantilla">Identificador de la plantilla de radicacion.</param>
        /// <param name="tipo">Tipo de radicacion consultable: entrante o saliente.</param>
        /// <param name="cancellationToken">Token de cancelacion de la solicitud HTTP.</param>
        /// <returns>Contrato AppResponses con metadata de campos consultables normalizados.</returns>
        [HttpGet("plantillas/{idPlantilla:long}/estructura")]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoEstructuraDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoEstructuraDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoEstructuraDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AppResponses<ConsultaPreviaRadicadoEstructuraDto>>> GetEstructuraAsync(
            long idPlantilla,
            [FromQuery] string tipo,
            CancellationToken cancellationToken)
        {
            var requestId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");

            try
            {
                var alias = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!alias.Success || string.IsNullOrWhiteSpace(alias.ClaimValue))
                {
                    return BadRequest(_errorFactory.Validation<ConsultaPreviaRadicadoEstructuraDto>(
                        ConsultaPreviaRadicadoErrorCatalog.ClaimRequired,
                        "defaulalias",
                        requestId));
                }

                var usuario = _claimValidationService.ValidateClaim<string>("usuarioid");
                if (!usuario.Success || string.IsNullOrWhiteSpace(usuario.ClaimValue)
                    || !long.TryParse(usuario.ClaimValue, out var usuarioId))
                {
                    return BadRequest(_errorFactory.Validation<ConsultaPreviaRadicadoEstructuraDto>(
                        ConsultaPreviaRadicadoErrorCatalog.ClaimRequired,
                        "usuarioid",
                        requestId));
                }

                var result = await _consultaPreviaRadicadoService.GetEstructuraAsync(
                    idPlantilla,
                    tipo,
                    alias.ClaimValue,
                    usuarioId,
                    requestId,
                    cancellationToken);

                if (result.success)
                {
                    return Ok(result);
                }

                return StatusCode(ResolveStatusCode(result), result);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var error = _errorFactory.Unexpected<ConsultaPreviaRadicadoEstructuraDto>(requestId, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        private static int ResolveStatusCode(AppResponses<ConsultaPreviaRadicadoEstructuraDto> response)
        {
            var code = response.errors?
                .OfType<MiApp.DTOs.DTOs.Errors.AppError>()
                .Select(error => error.Code)
                .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

            return ConsultaPreviaRadicadoErrorResponseFactory.ResolveHttpStatus(code);
        }
    }
}
