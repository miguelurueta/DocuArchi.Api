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
                        "defaulalias",
                        validation.Response?.message);

                    return BadRequest(claimResponse);
                }

                var result = await _service.ObtenerFormularioAsync(
                    request,
                    validation.ClaimValue,
                    requestId,
                    _currentUserService.UsuarioIdInt);

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

        /// <summary>
        /// Obtiene opciones de autocomplete para un campo dinamico de tercero/remitente.
        /// </summary>
        /// <param name="request">IdScript, nombre de campo y texto buscado.</param>
        /// <returns>Contrato AppResponses con opciones idValue/texValue del campo solicitado.</returns>
        [HttpPost("campos/autocomplete")]
        [ProducesResponseType(typeof(AppResponses<List<TerceroCampoAutocompleteOptionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<List<TerceroCampoAutocompleteOptionDto>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponses<List<TerceroCampoAutocompleteOptionDto>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AppResponses<List<TerceroCampoAutocompleteOptionDto>>>> AutocompleteCampo(
            [FromBody] TerceroCampoAutocompleteRequestDto request)
        {
            var requestId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");

            try
            {
                var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!validation.Success || string.IsNullOrWhiteSpace(validation.ClaimValue))
                {
                    var claimResponse = TercerosPlantillaValidacionErrorResponseFactory.Build<List<TerceroCampoAutocompleteOptionDto>>(
                        TercerosPlantillaValidacionErrorCatalog.AutocompleteClaimRequired,
                        requestId,
                        "defaulalias",
                        validation.Response?.message);

                    return BadRequest(claimResponse);
                }

                var result = await _service.AutocompleteCampoAsync(
                    request,
                    validation.ClaimValue,
                    requestId,
                    _currentUserService.UsuarioIdInt);

                if (!result.success)
                {
                    return StatusCode(TercerosPlantillaValidacionErrorResponseFactory.ResolveHttpStatus(result.meta?.Status), result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "terceros_autocomplete_controller_unexpected requestId={RequestId}", requestId);

                var response = TercerosPlantillaValidacionErrorResponseFactory.Build<List<TerceroCampoAutocompleteOptionDto>>(
                    TercerosPlantillaValidacionErrorCatalog.AutocompleteUnexpected,
                    requestId,
                    nameof(AutocompleteCampo),
                    ex.Message);

                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(AppResponses<TerceroOperacionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<TerceroOperacionResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponses<TerceroOperacionResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AppResponses<TerceroOperacionResponseDto>>> Crear(
            [FromBody] TerceroGuardarRequestDto request)
        {
            var requestId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");
            var aliasValidation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!aliasValidation.Success || string.IsNullOrWhiteSpace(aliasValidation.ClaimValue))
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
                    aliasValidation.ClaimValue,
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

        [HttpPut("{idRegistro:int}")]
        [ProducesResponseType(typeof(AppResponses<TerceroOperacionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<TerceroOperacionResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponses<TerceroOperacionResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AppResponses<TerceroOperacionResponseDto>>> Actualizar(
            [FromRoute] int idRegistro,
            [FromBody] TerceroActualizarRequestDto request)
        {
            var requestId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");
            var aliasValidation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!aliasValidation.Success || string.IsNullOrWhiteSpace(aliasValidation.ClaimValue))
            {
                var error = TercerosPlantillaValidacionErrorResponseFactory.Build<TerceroOperacionResponseDto>(
                    TercerosPlantillaValidacionErrorCatalog.EditarClaimRequired,
                    requestId,
                    "defaulalias",
                    aliasValidation.Response?.message);
                return BadRequest(error);
            }

            if (!_currentUserService.HasPermission(DocuArchiRAClaims.TerceroEditar))
            {
                var error = TercerosPlantillaValidacionErrorResponseFactory.Build<TerceroOperacionResponseDto>(
                    TercerosPlantillaValidacionErrorCatalog.EditarForbidden,
                    requestId,
                    "permission",
                    DocuArchiRAClaims.TerceroEditar);
                return BadRequest(error);
            }

            try
            {
                var response = await _service.ActualizarAsync(
                    idRegistro,
                    request,
                    aliasValidation.ClaimValue,
                    requestId,
                    _currentUserService.UsuarioIdInt);

                return response.success
                    ? Ok(response)
                    : StatusCode(TercerosPlantillaValidacionErrorResponseFactory.ResolveHttpStatus(response.meta?.Status), response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "terceros_editar_controller_unexpected requestId={RequestId} idRegistro={IdRegistro}", requestId, idRegistro);
                var error = TercerosPlantillaValidacionErrorResponseFactory.Build<TerceroOperacionResponseDto>(
                    TercerosPlantillaValidacionErrorCatalog.EditarUnexpected,
                    requestId,
                    "controller",
                    ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        [HttpDelete("{idRegistro:int}")]
        [ProducesResponseType(typeof(AppResponses<TerceroOperacionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<TerceroOperacionResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponses<TerceroOperacionResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AppResponses<TerceroOperacionResponseDto>>> Eliminar(
            [FromRoute] int idRegistro,
            [FromBody] TerceroEliminarRequestDto request)
        {
            var requestId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");
            var aliasValidation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!aliasValidation.Success || string.IsNullOrWhiteSpace(aliasValidation.ClaimValue))
            {
                var error = TercerosPlantillaValidacionErrorResponseFactory.Build<TerceroOperacionResponseDto>(
                    TercerosPlantillaValidacionErrorCatalog.EliminarClaimRequired,
                    requestId,
                    "defaulalias",
                    aliasValidation.Response?.message);
                return BadRequest(error);
            }

            if (!_currentUserService.HasPermission(DocuArchiRAClaims.TerceroEliminar))
            {
                var error = TercerosPlantillaValidacionErrorResponseFactory.Build<TerceroOperacionResponseDto>(
                    TercerosPlantillaValidacionErrorCatalog.EliminarForbidden,
                    requestId,
                    "permission",
                    DocuArchiRAClaims.TerceroEliminar);
                return BadRequest(error);
            }

            try
            {
                var response = await _service.EliminarAsync(
                    idRegistro,
                    request,
                    aliasValidation.ClaimValue,
                    requestId,
                    _currentUserService.UsuarioIdInt);

                return response.success
                    ? Ok(response)
                    : StatusCode(TercerosPlantillaValidacionErrorResponseFactory.ResolveHttpStatus(response.meta?.Status), response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "terceros_eliminar_controller_unexpected requestId={RequestId} idRegistro={IdRegistro}", requestId, idRegistro);
                var error = TercerosPlantillaValidacionErrorResponseFactory.Build<TerceroOperacionResponseDto>(
                    TercerosPlantillaValidacionErrorCatalog.EliminarUnexpected,
                    requestId,
                    "controller",
                    ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        private static int ResolveStatus(AppResponses<TerceroFormularioResponseDto> result)
        {
            return TercerosPlantillaValidacionErrorResponseFactory.ResolveHttpStatus(result.meta?.Status);
        }
    }
}
