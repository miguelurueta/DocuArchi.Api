using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.Radicacion.Configuracion;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.Radicacion.Configuracion;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.Radicacion.Configuracion
{
    [Route("api/configuracionPlantilla")]
    [ApiController]
    public class ConfiguracionPlantillaController : Controller
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IConfiguracionPlantillaService _configuracionPlantillaService;

        public ConfiguracionPlantillaController(
            IClaimValidationService claimValidationService,
            IConfiguracionPlantillaService configuracionPlantillaService)
        {
            _claimValidationService = claimValidationService;
            _configuracionPlantillaService = configuracionPlantillaService;
        }

        /// <summary>
        /// Consulta la configuración de plantilla de radicación por plantilla y tipo de radicación.
        /// </summary>
        /// <param name="idPlantilla">Id de plantilla.</param>
        /// <param name="tipoRadicacionPlantilla">Tipo de radicación asociado a la plantilla.</param>
        /// <returns>AppResponses con configuración encontrada o "Sin resultados".</returns>
        [HttpGet("solicitaConfiguracionPlantilla")]
        public async Task<ActionResult<AppResponses<RaRadConfigPlantillaRadicacionDto?>>> SolicitaConfiguracionPlantilla(
            int idPlantilla,
            int tipoRadicacionPlantilla)
        {
            try
            {
                var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!validation.Success || validation.ClaimValue == null)
                {
                    return BadRequest(validation.Response);
                }

                var result = await _configuracionPlantillaService.SolicitaConfiguracionPlantillaAsync(
                    idPlantilla,
                    tipoRadicacionPlantilla,
                    validation.ClaimValue);

                if (!result.success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new AppResponses<RaRadConfigPlantillaRadicacionDto?>
                    {
                        success = false,
                        message = "Error inesperado al consultar configuración de plantilla",
                        data = null,
                        errors =
                        [
                            new AppError
                            {
                                Type = "Exception",
                                Field = "idPlantilla",
                                Message = ex.Message
                            }
                        ]
                    });
            }
        }
    }
}
