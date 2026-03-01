using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.General;
using MiApp.DTOs.DTOs.GestorDocumental.usuario;
using MiApp.DTOs.DTOs.Home.Menu;
using MiApp.DTOs.DTOs.Radicacion.Tramite;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Repository.ErrorController;
using MiApp.Repository.Repositorio.Radicador.PlantillaRadicado;
using MiApp.Repository.Repositorio.Radicador.PlantillaValidacion;
using MiApp.Services.Service.GestorDocumental.Usuario;
using MiApp.Services.Service.Radicacion.PlantillaRadicado;
using MiApp.Services.Service.Radicacion.PlantillaValidacion;
using MiApp.Services.Service.Radicacion.Tramite;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security;


namespace DocuArchi.Api.Controllers.Radicacion.PlantillaRadicado
{
    [Route("api/PlantillaRadicado")]
    [ApiController]
    [Authorize] // 👈 OBLIGATORIO
    public class PlantillaRadicacionController : Controller
    {
        private readonly ICurrentUserService _ICurrentUserService;
        private readonly IPlantillaRadicacionL _IPlantillaRadicacionL;
        private readonly IPlantillaValidacionR _IPlantillaValidacionR;
        private readonly IClaimValidationService _claimValidationService;
        private readonly IPlantillaValidacionL _PlantillaValidacionL;
        private readonly IUsuarioCaracterizacionService _usuarioCaracterizacionService;
        private readonly IAutoCompleteDestinatarioRestriccionService _autoCompleteDestinatarioRestriccionService;
        private readonly ICamposDinamicosPlantillaService _camposDinamicosPlantillaService;
        public PlantillaRadicacionController(ICurrentUserService iCurrentUserService, IPlantillaRadicacionL iPlantillaRadicacionL, IClaimValidationService claimValidationService, IPlantillaValidacionR iPlantillaValidacionR, IPlantillaValidacionL plantillaValidacionL, IUsuarioCaracterizacionService usuarioCaracterizacionService, IAutoCompleteDestinatarioRestriccionService autoCompleteDestinatarioRestriccionService, ICamposDinamicosPlantillaService camposDinamicosPlantillaService)
        {
            _ICurrentUserService = iCurrentUserService;
            _IPlantillaRadicacionL = iPlantillaRadicacionL;
            _claimValidationService = claimValidationService;
            _IPlantillaValidacionR = iPlantillaValidacionR;
            _PlantillaValidacionL = plantillaValidacionL;
            _usuarioCaracterizacionService = usuarioCaracterizacionService;
            _autoCompleteDestinatarioRestriccionService = autoCompleteDestinatarioRestriccionService;
            _camposDinamicosPlantillaService = camposDinamicosPlantillaService;
        }

        [HttpGet("listaPlantilla")]
        public async Task<ActionResult<AppResponses<List<Class_config_general_service>>>>
           SolicitaEstructuraCamposRadicacion()
        {
            try
            {
               
                var validation = _claimValidationService.ValidateClaim<List<Class_config_general_service>>("defaulalias");
                if (!validation.Success || validation.ClaimValue == null)
                {       return
                        BadRequest(validation.Response);
                }
                    var result = await _IPlantillaRadicacionL.SolicitaEstructuraCamposRadicacion(validation.ClaimValue);
                    if (!result.success) return BadRequest(result);
                    return Ok(result);
                
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new AppResponses<List<Class_config_general_service>>
                    {
                        success = false,
                        message = "Error inesperado al cargar la plantilla de radicacion." + ex.Message,
                        data = null,
                        errors = new[]
                        {
                            new AppError
                            {
                                Type = "System",
                                Field = "",
                                Message = ex.Message
                            }
                        }
                    });
            }
        }

        [HttpPost("autoCompleteTercero")]
        public async Task<ActionResult<AppResponses<List<rowTomSelect>>>>
        SolicitaDatosAutoCompleteTerceroPlantillaController(ParameterDropDonws parameterDropDonws)
        {
            try
            {
                // Validación de claims
                var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!validation.Success || validation.ClaimValue == null)
                {
                    return BadRequest(validation.Response);
                }

                // Construir configuración base
                var resRemitente = await _PlantillaValidacionL.ConstruirEstructuraConsultaAutoCompletePlantValidacion(
                    parameterDropDonws.idScript,
                    validation.ClaimValue,
                    ""
                );

                if (!resRemitente.success || resRemitente.data == null || !resRemitente.data.Any())
                {
                    return BadRequest(resRemitente);
                }

                // Tomamos el primer objeto de configuración (puedes ajustar si necesitas varios)
                var configService = resRemitente.data.First();

                // Llamamos al servicio de autocompletado con el DTO completo
                var result = await _IPlantillaValidacionR.SolicitaDatosAutoCompleteTerceroPlantilla(
                    configService,
                    parameterDropDonws.valueCampo, ""
                );

                if (!result.Success)
                    return BadRequest(result);

                    return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new AppResponses<List<rowTomSelect>>
                    {
                        success = false,
                        message = "Error inesperado al cargar datos de autocompletado. " + ex.Message,
                        data = null,
                        errors = new[] { 
                            new AppError
                            {
                                Type="Error controller",
                                Field="",
                                Message="Error inesperado al cargar datos de autocompletado. " + ex.Message

                            }
                        } 
                    });
            }
        }
        [HttpPost("caracterizacionDestinatario")]
        public async Task<ActionResult<AppResponses<List<UsuarioCaracterizacionDto>>>> GetCaracterizacionUsuarios(int idRemitDestInterno)
        {
            try
            {
                var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!validation.Success || validation.ClaimValue == null)
                {
                    return BadRequest(validation.Response);
                }

                var result = await _usuarioCaracterizacionService.ObtenerCaracterizacionUsuariosAsync(validation.ClaimValue, idRemitDestInterno);

                if (!result.success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new AppResponses<List<UsuarioCaracterizacionDto>>
                    {
                        success = false,
                        message = "Error inesperado al cargar datos de caracterización usuario. " + ex.Message,
                        data = null,
                        errors = new[] {
                            new AppError
                            {
                                Type="Error controller",
                                Field="",
                                Message="Error inesperado al cargar datos de caracterización usuario. " + ex.Message

                            }
                        }
                    });
            }
        }
        [HttpPost("solicitaAutoCompleteDestinatarioRestriccion")]
        public async Task<ActionResult<AppResponses<List<rowTomSelect>>>> EmpsolicitaAutoCompleteDestinatarioRestriccion(
        [FromBody] parameterRestricionDestinatario param)
        {
            var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!validation.Success || validation.ClaimValue == null)
            {
                return BadRequest(validation.Response);
            }
            var defaulalias = validation.ClaimValue;
            validation  = _claimValidationService.ValidateClaim<string>("usuarioid");
            if (!validation.Success || validation.ClaimValue == null)
            {
                return BadRequest(validation.Response);
            }
            if (!int.TryParse(validation.ClaimValue, out var idUsuarioGestion))
                throw new SecurityException("Claim inválido: idUsuarioGestion");
          var result = await _autoCompleteDestinatarioRestriccionService.ServiceSolicitaAutoCompleteDestinatarioRestriccion(param,
                idUsuarioGestion,
                defaulalias);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("solicitaAutoCompleteCampos")]
        public async Task<ActionResult<AppResponses<List<ReturnAutoComplete>>>> EmpsolicitaAutoCompleteDestinatarioRestriccion(
        [FromBody] ParameterAutoComplete parameterAutoComplete)
        {
            var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!validation.Success || validation.ClaimValue == null)
            {
                return BadRequest(validation.Response);
            }

            parameterAutoComplete.defaultDbAlias = validation.ClaimValue;

            var result = await _camposDinamicosPlantillaService.ServiceSolicitaAutoCompleteCamposDinamicos(parameterAutoComplete);

            if (!result.success)
                return BadRequest(result);

            return Ok(result);
        }


    }
}
