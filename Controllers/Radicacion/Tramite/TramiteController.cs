using MiApp.DTOs.DTOs.General;
using MiApp.DTOs.DTOs.Radicacion.Tramite;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.Radicacion.Tramite;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        public TramiteController(
            IClaimValidationService claimValidationService,
            IFlujosRelacionadosTramiteService flujosRelacionadosTramiteService,
            IRelacionTipoRestriccionService relacionTipoRestriccionService,
            ITotalDiasVencimientoTramiteService totalDiasVencimientoTramiteService)
        {
            _claimValidationService = claimValidationService;
            _flujosRelacionadosTramiteService = flujosRelacionadosTramiteService;
            _relacionTipoRestriccionService = relacionTipoRestriccionService;
            _totalDiasVencimientoTramiteService = totalDiasVencimientoTramiteService;
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
    }
}
