using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.General;
using MiApp.DTOs.DTOs.GestorDocumental.usuario;
using MiApp.DTOs.DTOs.Home.Menu;
using MiApp.DTOs.DTOs.Radicacion.PlatillaRadicado;
using MiApp.DTOs.DTOs.Radicacion.PlantillaValidacion;
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
        private readonly ISolicitaAutoCompleteTokenRadicadoService _solicitaAutoCompleteTokenRadicadoService;
        private readonly ISolicitaAutoCompleteTokenExpedienteRadicadoService _solicitaAutoCompleteTokenExpedienteRadicadoService;
        private readonly IAsignacionCamposValidacionDinamicaService _asignacionCamposValidacionDinamicaService;
        private readonly IPlantillaRadicacionDefaultService _plantillaRadicacionDefaultService;
        public PlantillaRadicacionController(ICurrentUserService iCurrentUserService, IPlantillaRadicacionL iPlantillaRadicacionL, IClaimValidationService claimValidationService, IPlantillaValidacionR iPlantillaValidacionR, IPlantillaValidacionL plantillaValidacionL, IUsuarioCaracterizacionService usuarioCaracterizacionService, IAutoCompleteDestinatarioRestriccionService autoCompleteDestinatarioRestriccionService, ICamposDinamicosPlantillaService camposDinamicosPlantillaService, ISolicitaAutoCompleteTokenRadicadoService solicitaAutoCompleteTokenRadicadoService, ISolicitaAutoCompleteTokenExpedienteRadicadoService solicitaAutoCompleteTokenExpedienteRadicadoService, IAsignacionCamposValidacionDinamicaService asignacionCamposValidacionDinamicaService, IPlantillaRadicacionDefaultService plantillaRadicacionDefaultService)
        {
            _ICurrentUserService = iCurrentUserService;
            _IPlantillaRadicacionL = iPlantillaRadicacionL;
            _claimValidationService = claimValidationService;
            _IPlantillaValidacionR = iPlantillaValidacionR;
            _PlantillaValidacionL = plantillaValidacionL;
            _usuarioCaracterizacionService = usuarioCaracterizacionService;
            _autoCompleteDestinatarioRestriccionService = autoCompleteDestinatarioRestriccionService;
            _camposDinamicosPlantillaService = camposDinamicosPlantillaService;
            _solicitaAutoCompleteTokenRadicadoService = solicitaAutoCompleteTokenRadicadoService;
            _solicitaAutoCompleteTokenExpedienteRadicadoService = solicitaAutoCompleteTokenExpedienteRadicadoService;
            _asignacionCamposValidacionDinamicaService = asignacionCamposValidacionDinamicaService;
            _plantillaRadicacionDefaultService = plantillaRadicacionDefaultService;
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

        /// <summary>
        /// Consulta la plantilla de radicacion configurada como default para el alias del usuario autenticado.
        /// </summary>
        /// <returns>Id, nombre y tipo de la plantilla default de radicacion.</returns>
        [HttpGet("plantilla-default")]
        [ProducesResponseType(typeof(AppResponses<PlantillaRadicacionDefaultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<PlantillaRadicacionDefaultDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponses<PlantillaRadicacionDefaultDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AppResponses<PlantillaRadicacionDefaultDto>>> SolicitaPlantillaDefaultRadicacion()
        {
            try
            {
                var requestId = ResolveRequestId();
                var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!validation.Success || validation.ClaimValue == null)
                {
                    return BadRequest(validation.Response);
                }

                var result = await _plantillaRadicacionDefaultService.SolicitarPlantillaDefaultAsync(
                    validation.ClaimValue,
                    requestId);

                if (!result.success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch
            {
                var requestId = ResolveRequestId();
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new AppResponses<PlantillaRadicacionDefaultDto>
                    {
                        success = false,
                        message = "No fue posible consultar la plantilla de radicacion por defecto.",
                        data = null,
                        meta = new MiApp.DTOs.DTOs.Utilidades.AppMeta
                        {
                            Status = PlantillaRadicacionDefaultService.UnexpectedCode,
                            RequestId = requestId
                        },
                        errors = new object[]
                        {
                            new AppError
                            {
                                Type = "Technical",
                                Field = PlantillaRadicacionDefaultService.UnexpectedCode,
                                Code = PlantillaRadicacionDefaultService.UnexpectedCode,
                                Message = "No fue posible consultar la plantilla de radicacion por defecto.",
                                UserMessage = "No fue posible consultar la plantilla de radicacion por defecto."
                            }
                        }
                    });
            }
        }

        private string ResolveRequestId()
        {
            return ControllerContext.HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// Asigna campos destino de radicacion a partir de un registro seleccionado en una plantilla de validacion dinamica.
        /// </summary>
        /// <param name="request">Datos del script, campo origen, plantilla de radicacion y registro de validacion seleccionado.</param>
        /// <returns>Campos destino normalizados, marcados como solo lectura y con origen VALIDACION_DINAMICA.</returns>
        [HttpPost("validacion-dinamica/asignar-campos")]
        [ProducesResponseType(typeof(AppResponses<AsignacionCamposValidacionDinamicaResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<AsignacionCamposValidacionDinamicaResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponses<AsignacionCamposValidacionDinamicaResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AppResponses<AsignacionCamposValidacionDinamicaResponseDto>>> AsignarCamposValidacionDinamica(
            [FromBody] AsignacionCamposValidacionDinamicaRequestDto request)
        {
            try
            {
                var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!validation.Success || validation.ClaimValue == null)
                {
                    return BadRequest(validation.Response);
                }

                var result = await _asignacionCamposValidacionDinamicaService.AsignarCamposAsync(
                    request,
                    validation.ClaimValue,
                    HttpContext.TraceIdentifier);

                if (!result.success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new AppResponses<AsignacionCamposValidacionDinamicaResponseDto>
                    {
                        success = false,
                        message = "VALIDACION_DINAMICA_UNEXPECTED: Error inesperado al asignar campos de validacion dinamica.",
                        data = new AsignacionCamposValidacionDinamicaResponseDto(),
                        meta = new MiApp.DTOs.DTOs.Utilidades.AppMeta
                        {
                            Status = "VALIDACION_DINAMICA_UNEXPECTED",
                            RequestId = HttpContext.TraceIdentifier
                        },
                        errors = new object[]
                        {
                            new AppError
                            {
                                Type = "Technical",
                                Code = "VALIDACION_DINAMICA_UNEXPECTED",
                                Field = "AsignarCamposValidacionDinamica",
                                Message = "Error inesperado al asignar campos de validacion dinamica.",
                                UserMessage = "No fue posible asignar los campos de validacion dinamica."
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

        /// <summary>
        /// Busca coincidencias de token de radicado sobre el campo consecutivo_rad de la plantilla por defecto.
        /// </summary>
        /// <param name="parameterAutoComplete">Texto y metadatos del autocomplete enviados por frontend.</param>
        /// <returns>Resultado AppResponses con lista de rowTomSelect.</returns>
        [HttpPost("solicitaAutoCompleteTokenRadicado")]
        public async Task<ActionResult<AppResponses<List<rowTomSelect>>>> SolicitaAutoCompleteTokenRadicado(
            [FromBody] ParameterAutoComplete parameterAutoComplete)
        {
            try
            {
                var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!validation.Success || validation.ClaimValue == null)
                {
                    return BadRequest(validation.Response);
                }

                parameterAutoComplete ??= new ParameterAutoComplete();
                parameterAutoComplete.defaultDbAlias = validation.ClaimValue;

                var result = await _solicitaAutoCompleteTokenRadicadoService
                    .ServiceSolicitaAutoCompleteTokenRadicadoAsync(parameterAutoComplete, parameterAutoComplete.defaultDbAlias);

                if (!result.success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new AppResponses<List<rowTomSelect>>
                    {
                        success = false,
                        message = "Error inesperado al consultar token radicado",
                        data = [],
                        errors = new[]
                        {
                            new AppError
                            {
                                Type = "Technical",
                                Field = "SolicitaAutoCompleteTokenRadicado",
                                Message = ex.Message
                            }
                        }
                    });
            }
        }

        /// <summary>
        /// Busca coincidencias de token de expediente sobre la tabla expediente_archivo.
        /// </summary>
        /// <param name="parameterAutoComplete">Texto y metadatos del autocomplete enviados por frontend.</param>
        /// <returns>Resultado AppResponses con lista de rowTomSelect.</returns>
        [HttpPost("solicitaAutoCompleteTokenExpedienteRadicado")]
        public async Task<ActionResult<AppResponses<List<rowTomSelect>>>> SolicitaAutoCompleteTokenExpedienteRadicado(
            [FromBody] ParameterAutoComplete parameterAutoComplete)
        {
            try
            {
                var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!validation.Success || validation.ClaimValue == null)
                {
                    return BadRequest(validation.Response);
                }

                parameterAutoComplete ??= new ParameterAutoComplete();
                parameterAutoComplete.defaultDbAlias = validation.ClaimValue;

                var result = await _solicitaAutoCompleteTokenExpedienteRadicadoService
                    .ServiceSolicitaAutoCompleteTokenExpedienteRadicadoAsync(parameterAutoComplete, validation.ClaimValue);

                if (!result.success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new AppResponses<List<rowTomSelect>>
                    {
                        success = false,
                        message = "Error inesperado al consultar token expediente radicado",
                        data = [],
                        errors = new[]
                        {
                            new AppError
                            {
                                Type = "Technical",
                                Field = "SolicitaAutoCompleteTokenExpedienteRadicado",
                                Message = ex.Message
                            }
                        }
                    });
            }
        }


    }
}
