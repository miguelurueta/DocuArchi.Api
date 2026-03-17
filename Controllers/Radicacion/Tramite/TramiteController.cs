using MiApp.DTOs.DTOs.General;
using MiApp.DTOs.DTOs.Radicacion.Tramite;
using MiApp.DTOs.DTOs.UI.MuiTable;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.Radicacion.Tramite;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Mvc;
using System.Security;

namespace DocuArchi.Api.Controllers.Radicacion.Tramite
{
    [Route("api/tramite")]
    [ApiController]
    //[Authorize] // 👈 OBLIGATORIO
    public class TramiteController : Controller
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IFlujosRelacionadosTramiteService _flujosRelacionadosTramiteService;
        private readonly IRelacionTipoRestriccionService _relacionTipoRestriccionService;
        private readonly ITotalDiasVencimientoTramiteService _totalDiasVencimientoTramiteService;
        private readonly IListaDiasFeriadosTramiteService _listaDiasFeriadosTramiteService;
        private readonly IFechaLimiteRespuestaService _fechaLimiteRespuestaService;
        private readonly IListaRadicadosPendientesService _listaRadicadosPendientesService;

        public TramiteController(
            IClaimValidationService claimValidationService,
            IFlujosRelacionadosTramiteService flujosRelacionadosTramiteService,
            IRelacionTipoRestriccionService relacionTipoRestriccionService,
            ITotalDiasVencimientoTramiteService totalDiasVencimientoTramiteService,
            IListaDiasFeriadosTramiteService listaDiasFeriadosTramiteService,
            IFechaLimiteRespuestaService fechaLimiteRespuestaService,
            IListaRadicadosPendientesService listaRadicadosPendientesService)
        {
            _claimValidationService = claimValidationService;
            _flujosRelacionadosTramiteService = flujosRelacionadosTramiteService;
            _relacionTipoRestriccionService = relacionTipoRestriccionService;
            _totalDiasVencimientoTramiteService = totalDiasVencimientoTramiteService;
            _listaDiasFeriadosTramiteService = listaDiasFeriadosTramiteService;
            _fechaLimiteRespuestaService = fechaLimiteRespuestaService;
            _listaRadicadosPendientesService = listaRadicadosPendientesService;
        }

        [HttpGet("tramites/empsolicitaListaflujosRelacionadosTramite")]
        public async Task<ActionResult<AppResponses<List<serviceIlistdrow>>>> EmpsolicitaListaflujosRelacionadosTramite(int idTipoDocEntrante)
        {
            var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!validation.Success || validation.ClaimValue == null)
            {
                return BadRequest(validation.Response);
            }

            var result = await _flujosRelacionadosTramiteService.ObtenerFlujosRelacionadosTramiteAsync(validation.ClaimValue, idTipoDocEntrante);

            if (!result.success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("tramites/solicitaEstructuraRelacionTipoRestriccion")]
        public async Task<ActionResult<AppResponses<CDeRelacionEstadoRetriccionDto>>> EmpsolicitaEstructuraRelacionTipoRestriccion(int idTipoTramite)
        {
            var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!validation.Success || validation.ClaimValue == null)
            {
                return BadRequest(validation.Response);
            }

            var result = await _relacionTipoRestriccionService.ServiceSolicitaEstructuraRelacionTipoRestriccion(idTipoTramite, validation.ClaimValue);

            if (!result.success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("tramites/solicitaTotalDiasVencimientoTramite")]
        public async Task<ActionResult<AppResponses<int>>> SolicitaTotalDiasVencimientoTramite(int idPlantilla, int idTipoTramite)
        {
            var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!validation.Success || validation.ClaimValue == null)
            {
                return BadRequest(validation.Response);
            }

            var result = await _totalDiasVencimientoTramiteService
                .ServiceSolicitaTotalDiasVencimientoTramite(idPlantilla, idTipoTramite, validation.ClaimValue);

            if (!result.success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("tramites/solicitaListaDiasFeriados")]
        public async Task<ActionResult<AppResponses<List<string>>>> SolicitaListaDiasFeriados()
        {
            var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!validation.Success || validation.ClaimValue == null)
            {
                return BadRequest(validation.Response);
            }

            var result = await _listaDiasFeriadosTramiteService.ServiceSolicitaListaDiasFeriados(validation.ClaimValue);

            if (!result.success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("tramites/solicitaFechaLimiteRespuesta")]
        public async Task<ActionResult<AppResponses<FechaLimiteRespuestaDto>>> SolicitaFechaLimiteRespuesta(int idTipoTramite)
        {
            var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!validation.Success || validation.ClaimValue == null)
            {
                return BadRequest(validation.Response);
            }

            var result = await _fechaLimiteRespuestaService.SolicitaFechaLimiteRespuesta(idTipoTramite, validation.ClaimValue);

            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("tramites/apListaRadicadosPendientes")]
        public async Task<ActionResult<AppResponses<DynamicUiTableDto>>> ApListaRadicadosPendientes()
        {
            try
            {
                //var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
                //if (!validation.Success || validation.ClaimValue == null)
                //{
                //    return BadRequest(validation.Response);
                //}

                //var validationUsuario = _claimValidationService.ValidateClaim<string>("usuarioid");
                //if (!validationUsuario.Success || validationUsuario.ClaimValue == null)
                //{
                //    return BadRequest(validationUsuario.Response);
                //}

                //if (!int.TryParse(validationUsuario.ClaimValue, out var idUsuarioGestion))
                //{
                //    throw new SecurityException("Claim invalido: usuarioid");
                //}

                var result = await _listaRadicadosPendientesService
                    .SolicitaListaRadicadosPendientes(141, "DA");
                if (!result.success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new AppResponses<DynamicUiTableDto>
                    {
                        success = false,
                        message = "Error inesperado al consultar radicados pendientes",
                        errors =
                        [
                            new
                            {
                                Type = "Exception",
                                Field = "usuarioid",
                                Message = ex.Message
                            }
                        ],
                        data = null!
                    });
            }
        }
    }
}

