using MiApp.DTOs.DTOs.Radicacion.Tramite;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.Radicacion.Tramite;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security;

namespace DocuArchi.Api.Controllers.Radicacion.Tramite
{
    [Route("api/radicacion")]
    [ApiController]
    //[Authorize]
    public sealed class RadicacionController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IRegistrarRadicacionEntranteService _registrarService;
        private readonly IValidarRadicacionEntranteService _validarService;
        private readonly IFlujoInicialRadicacionService _flujoInicialService;

        public RadicacionController(
            IClaimValidationService claimValidationService,
            IRegistrarRadicacionEntranteService registrarService,
            IValidarRadicacionEntranteService validarService,
            IFlujoInicialRadicacionService flujoInicialService)
        {
            _claimValidationService = claimValidationService;
            _registrarService = registrarService;
            _validarService = validarService;
            _flujoInicialService = flujoInicialService;
        }

        [HttpPost("registrar-entrante")]
        public async Task<ActionResult<AppResponses<RegistrarRadicacionEntranteResponseDto>>> RegistrarEntrante(
            [FromBody] RegistrarRadicacionEntranteRequestDto request)
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

            var result = await _registrarService.RegistrarRadicacionEntranteAsync(
                request,
                idUsuarioGestion,
                aliasValidation.ClaimValue);
            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
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
