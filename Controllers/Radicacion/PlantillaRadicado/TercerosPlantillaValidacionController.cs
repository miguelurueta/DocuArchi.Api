using MiApp.DTOs.DTOs.Radicacion.PlantillaValidacion;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.Radicacion.PlantillaValidacion;
using MiApp.Services.Service.Radicacion.PlantillaValidacion.Errors;
using MiApp.Services.Service.Seguridad.Autorizacion.Claims;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.Radicacion.PlantillaRadicado
{
    [ApiController]
    [Authorize]
    [Route("api/PlantillaRadicado/terceros")]
    [Produces("application/json")]
    public sealed class TercerosPlantillaValidacionController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly ITercerosPlantillaValidacionService _service;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<TercerosPlantillaValidacionController> _logger;

        public TercerosPlantillaValidacionController(
            IClaimValidationService claimValidationService,
            ITercerosPlantillaValidacionService service,
            ICurrentUserService currentUserService,
            ILogger<TercerosPlantillaValidacionController> logger)
        {
            _claimValidationService = claimValidationService;
            _service = service;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        [HttpPost("formulario")]
        [ProducesResponseType(typeof(AppResponses<TerceroFormularioResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<TerceroFormularioResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponses<TerceroFormularioResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AppResponses<TerceroFormularioResponseDto>>> ObtenerFormulario(
            [FromBody] TerceroFormularioRequestDto request)
        {
            var requestId = HttpContext.TraceIdentifier;
            var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!validation.Success)
            {
                return BadRequest(validation.Response);
            }

            var response = await _service.ObtenerFormularioAsync(
                request,
                validation.ClaimValue ?? string.Empty,
                requestId,
                _currentUserService.UsuarioIdInt);

            return response.success
                ? Ok(response)
                : StatusCode(TercerosPlantillaValidacionErrorResponseFactory.ResolveHttpStatus(response.meta?.Status), response);
        }

        [HttpPost]
        [ProducesResponseType(typeof(AppResponses<TerceroOperacionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<TerceroOperacionResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponses<TerceroOperacionResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AppResponses<TerceroOperacionResponseDto>>> Crear(
            [FromBody] TerceroGuardarRequestDto request)
        {
            var requestId = HttpContext.TraceIdentifier;
            var aliasValidation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!aliasValidation.Success)
            {
                var error = TercerosPlantillaValidacionErrorResponseFactory.Build<TerceroOperacionResponseDto>(
                    TercerosPlantillaValidacionErrorCatalog.CrearClaimRequired,
                    requestId,
                    "defaulalias",
                    aliasValidation.Response?.message);
                return BadRequest(error);
            }

            if (!_currentUserService.HasPermission(DocuArchiRAClaims.TerceroAdicionar))
            {
                var error = TercerosPlantillaValidacionErrorResponseFactory.Build<TerceroOperacionResponseDto>(
                    TercerosPlantillaValidacionErrorCatalog.CrearForbidden,
                    requestId,
                    "permission",
                    DocuArchiRAClaims.TerceroAdicionar);
                return BadRequest(error);
            }

            try
            {
                var response = await _service.CrearAsync(
                    request,
                    aliasValidation.ClaimValue ?? string.Empty,
                    requestId,
                    _currentUserService.UsuarioIdInt);

                return response.success
                    ? Ok(response)
                    : StatusCode(TercerosPlantillaValidacionErrorResponseFactory.ResolveHttpStatus(response.meta?.Status), response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "terceros_crear_controller_unexpected requestId={RequestId}", requestId);
                var error = TercerosPlantillaValidacionErrorResponseFactory.Build<TerceroOperacionResponseDto>(
                    TercerosPlantillaValidacionErrorCatalog.CrearUnexpected,
                    requestId,
                    "controller",
                    ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }
    }
}
