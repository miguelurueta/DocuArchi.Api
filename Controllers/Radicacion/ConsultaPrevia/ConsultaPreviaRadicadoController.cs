using MiApp.DTOs.DTOs.Radicacion.ConsultaPrevia;
using MiApp.DTOs.DTOs.UI.MuiTable;
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

        /// <summary>
        /// Consulta radicados previos con filtros dinamicos y respuesta compatible con AppTable.
        /// </summary>
        /// <param name="request">Request paginado de consulta previa.</param>
        /// <param name="cancellationToken">Token de cancelacion de la solicitud HTTP.</param>
        /// <returns>Contrato AppResponses con DynamicUiTableDto.</returns>
        [HttpPost("query")]
        [ProducesResponseType(typeof(AppResponses<DynamicUiTableDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<DynamicUiTableDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponses<DynamicUiTableDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AppResponses<DynamicUiTableDto>>> QueryAsync(
            [FromBody] ConsultaPreviaRadicadoQueryRequestDto request,
            CancellationToken cancellationToken)
        {
            var requestId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");

            try
            {
                if (request == null)
                {
                    return BadRequest(_errorFactory.Validation<DynamicUiTableDto>(
                        ConsultaPreviaRadicadoErrorCatalog.QueryRequestInvalido,
                        "request",
                        requestId));
                }

                var alias = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!alias.Success || string.IsNullOrWhiteSpace(alias.ClaimValue))
                {
                    return BadRequest(_errorFactory.Validation<DynamicUiTableDto>(
                        ConsultaPreviaRadicadoErrorCatalog.QueryClaimRequired,
                        "defaulalias",
                        requestId));
                }

                var usuario = _claimValidationService.ValidateClaim<string>("usuarioid");
                if (!usuario.Success || string.IsNullOrWhiteSpace(usuario.ClaimValue)
                    || !long.TryParse(usuario.ClaimValue, out var usuarioId))
                {
                    return BadRequest(_errorFactory.Validation<DynamicUiTableDto>(
                        ConsultaPreviaRadicadoErrorCatalog.QueryClaimRequired,
                        "usuarioid",
                        requestId));
                }

                var result = await _consultaPreviaRadicadoService.QueryAsync(
                    request,
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
                var error = _errorFactory.FromDescriptor<DynamicUiTableDto>(
                    ConsultaPreviaRadicadoErrorCatalog.Get(ConsultaPreviaRadicadoErrorCatalog.QueryUnexpected),
                    "unexpected",
                    requestId,
                    ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        /// <summary>
        /// Busca sugerencias autorizadas para un campo de consulta previa.
        /// </summary>
        /// <param name="idPlantilla">Identificador de la plantilla de radicacion.</param>
        /// <param name="nombreCanonico">Nombre canonico publicado por la estructura.</param>
        /// <param name="request">Tipo, texto, limite opcional y contexto v1 vacío.</param>
        /// <param name="cancellationToken">Token de cancelacion de la solicitud HTTP.</param>
        [HttpPost("plantillas/{idPlantilla:long}/campos/{nombreCanonico}/autocomplete")]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoAutocompleteResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoAutocompleteResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoAutocompleteResponseDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoAutocompleteResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AppResponses<ConsultaPreviaRadicadoAutocompleteResponseDto>>> GetAutocompleteAsync(
            long idPlantilla,
            string nombreCanonico,
            [FromBody] ConsultaPreviaRadicadoAutocompleteRequestDto request,
            CancellationToken cancellationToken)
        {
            var requestId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");

            try
            {
                if (request == null)
                {
                    return BadRequest(_errorFactory.Validation<ConsultaPreviaRadicadoAutocompleteResponseDto>(
                        ConsultaPreviaRadicadoErrorCatalog.AutocompleteRequestInvalido,
                        "request",
                        requestId));
                }

                var alias = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!alias.Success || string.IsNullOrWhiteSpace(alias.ClaimValue))
                {
                    return BadRequest(_errorFactory.Validation<ConsultaPreviaRadicadoAutocompleteResponseDto>(
                        ConsultaPreviaRadicadoErrorCatalog.AutocompleteClaimRequired,
                        "defaulalias",
                        requestId));
                }

                var usuario = _claimValidationService.ValidateClaim<string>("usuarioid");
                if (!usuario.Success || string.IsNullOrWhiteSpace(usuario.ClaimValue)
                    || !long.TryParse(usuario.ClaimValue, out var usuarioId))
                {
                    return BadRequest(_errorFactory.Validation<ConsultaPreviaRadicadoAutocompleteResponseDto>(
                        ConsultaPreviaRadicadoErrorCatalog.AutocompleteClaimRequired,
                        "usuarioid",
                        requestId));
                }

                var result = await _consultaPreviaRadicadoService.GetAutocompleteAsync(
                    idPlantilla,
                    nombreCanonico,
                    request,
                    alias.ClaimValue,
                    usuarioId,
                    requestId,
                    cancellationToken);

                return result.success ? Ok(result) : StatusCode(ResolveStatusCode(result), result);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var error = _errorFactory.FromDescriptor<ConsultaPreviaRadicadoAutocompleteResponseDto>(
                    ConsultaPreviaRadicadoErrorCatalog.Get(ConsultaPreviaRadicadoErrorCatalog.AutocompleteUnexpected),
                    "unexpected",
                    requestId,
                    ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        /// <summary>
        /// Obtiene el detalle vertical completo de un radicado consultado previamente.
        /// </summary>
        /// <param name="radicado">Consecutivo del radicado seleccionado.</param>
        /// <param name="idPlantilla">Identificador de plantilla de radicacion.</param>
        /// <param name="tipo">Tipo de radicacion: entrante o saliente.</param>
        /// <param name="cancellationToken">Token de cancelacion de la solicitud HTTP.</param>
        /// <returns>Contrato AppResponses con detalle vertical del radicado.</returns>
        [HttpGet("{radicado}/detalle")]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoDetalleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoDetalleDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoDetalleDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoDetalleDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoDetalleDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AppResponses<ConsultaPreviaRadicadoDetalleDto>>> GetDetalleAsync(
            string radicado,
            [FromQuery] long idPlantilla,
            [FromQuery] string tipo,
            CancellationToken cancellationToken)
        {
            var requestId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");

            try
            {
                var alias = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!alias.Success || string.IsNullOrWhiteSpace(alias.ClaimValue))
                {
                    return BadRequest(_errorFactory.Validation<ConsultaPreviaRadicadoDetalleDto>(
                        ConsultaPreviaRadicadoErrorCatalog.DetalleClaimRequired,
                        "defaulalias",
                        requestId));
                }

                var usuario = _claimValidationService.ValidateClaim<string>("usuarioid");
                if (!usuario.Success || string.IsNullOrWhiteSpace(usuario.ClaimValue)
                    || !long.TryParse(usuario.ClaimValue, out var usuarioId))
                {
                    return BadRequest(_errorFactory.Validation<ConsultaPreviaRadicadoDetalleDto>(
                        ConsultaPreviaRadicadoErrorCatalog.DetalleClaimRequired,
                        "usuarioid",
                        requestId));
                }

                var result = await _consultaPreviaRadicadoService.GetDetalleAsync(
                    radicado,
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
                var error = _errorFactory.FromDescriptor<ConsultaPreviaRadicadoDetalleDto>(
                    ConsultaPreviaRadicadoErrorCatalog.Get(ConsultaPreviaRadicadoErrorCatalog.DetalleUnexpected),
                    "unexpected",
                    requestId,
                    ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        /// <summary>
        /// Prepara los datos copiables de un radicado consultado hacia una plantilla destino sin persistir cambios.
        /// </summary>
        /// <param name="request">Request de preparacion de copia de datos.</param>
        /// <param name="cancellationToken">Token de cancelacion de la solicitud HTTP.</param>
        /// <returns>Contrato AppResponses con campos copiables, no mapeados y riesgo de sobrescritura.</returns>
        [HttpPost("copiar-datos")]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoCopiarDatosResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoCopiarDatosResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoCopiarDatosResponseDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoCopiarDatosResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoCopiarDatosResponseDto>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoCopiarDatosResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AppResponses<ConsultaPreviaRadicadoCopiarDatosResponseDto>>> CopiarDatosAsync(
            [FromBody] ConsultaPreviaRadicadoCopiarDatosRequestDto request,
            CancellationToken cancellationToken)
        {
            var requestId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");

            try
            {
                if (request == null)
                {
                    return BadRequest(_errorFactory.Validation<ConsultaPreviaRadicadoCopiarDatosResponseDto>(
                        ConsultaPreviaRadicadoErrorCatalog.CopiarRequestInvalido,
                        "request",
                        requestId));
                }

                var alias = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!alias.Success || string.IsNullOrWhiteSpace(alias.ClaimValue))
                {
                    return BadRequest(_errorFactory.Validation<ConsultaPreviaRadicadoCopiarDatosResponseDto>(
                        ConsultaPreviaRadicadoErrorCatalog.CopiarClaimRequired,
                        "defaulalias",
                        requestId));
                }

                var usuario = _claimValidationService.ValidateClaim<string>("usuarioid");
                if (!usuario.Success || string.IsNullOrWhiteSpace(usuario.ClaimValue)
                    || !long.TryParse(usuario.ClaimValue, out var usuarioId))
                {
                    return BadRequest(_errorFactory.Validation<ConsultaPreviaRadicadoCopiarDatosResponseDto>(
                        ConsultaPreviaRadicadoErrorCatalog.CopiarClaimRequired,
                        "usuarioid",
                        requestId));
                }

                var result = await _consultaPreviaRadicadoService.CopiarDatosAsync(
                    request,
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
                var error = _errorFactory.FromDescriptor<ConsultaPreviaRadicadoCopiarDatosResponseDto>(
                    ConsultaPreviaRadicadoErrorCatalog.Get(ConsultaPreviaRadicadoErrorCatalog.CopiarUnexpected),
                    "unexpected",
                    requestId,
                    ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        /// <summary>
        /// Anexa el radicado destino al expediente asociado al radicado origen.
        /// </summary>
        [HttpPost("anexar-expediente")]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoAnexarExpedienteResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoAnexarExpedienteResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoAnexarExpedienteResponseDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoAnexarExpedienteResponseDto>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoAnexarExpedienteResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoAnexarExpedienteResponseDto>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoAnexarExpedienteResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AppResponses<ConsultaPreviaRadicadoAnexarExpedienteResponseDto>>> AnexarExpedienteAsync(
            [FromBody] ConsultaPreviaRadicadoAnexarExpedienteRequestDto request,
            CancellationToken cancellationToken)
        {
            var requestId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");

            try
            {
                if (request == null)
                {
                    return BadRequest(_errorFactory.Validation<ConsultaPreviaRadicadoAnexarExpedienteResponseDto>(
                        ConsultaPreviaRadicadoErrorCatalog.RelacionRequestInvalido,
                        "request",
                        requestId));
                }

                var claims = ValidateBaseClaims<ConsultaPreviaRadicadoAnexarExpedienteResponseDto>(requestId);
                if (claims.Error != null)
                {
                    return BadRequest(claims.Error);
                }

                var result = await _consultaPreviaRadicadoService.AnexarExpedienteAsync(
                    request,
                    claims.DefaultDbAlias!,
                    claims.UsuarioId,
                    ExtractPermissionClaims(),
                    requestId,
                    cancellationToken);

                return result.success ? Ok(result) : StatusCode(ResolveStatusCode(result), result);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var error = _errorFactory.FromDescriptor<ConsultaPreviaRadicadoAnexarExpedienteResponseDto>(
                    ConsultaPreviaRadicadoErrorCatalog.Get(ConsultaPreviaRadicadoErrorCatalog.RelacionUnexpected),
                    "unexpected",
                    requestId,
                    ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        /// <summary>
        /// Relaciona dos radicados desde Consulta Previa con modo agrega o remplaza.
        /// </summary>
        [HttpPost("relacionar")]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoRelacionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoRelacionResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoRelacionResponseDto>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoRelacionResponseDto>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoRelacionResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoRelacionResponseDto>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(AppResponses<ConsultaPreviaRadicadoRelacionResponseDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AppResponses<ConsultaPreviaRadicadoRelacionResponseDto>>> RelacionarAsync(
            [FromBody] ConsultaPreviaRadicadoRelacionRequestDto request,
            CancellationToken cancellationToken)
        {
            var requestId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");

            try
            {
                if (request == null)
                {
                    return BadRequest(_errorFactory.Validation<ConsultaPreviaRadicadoRelacionResponseDto>(
                        ConsultaPreviaRadicadoErrorCatalog.RelacionRequestInvalido,
                        "request",
                        requestId));
                }

                var claims = ValidateBaseClaims<ConsultaPreviaRadicadoRelacionResponseDto>(requestId);
                if (claims.Error != null)
                {
                    return BadRequest(claims.Error);
                }

                var result = await _consultaPreviaRadicadoService.RelacionarAsync(
                    request,
                    claims.DefaultDbAlias!,
                    claims.UsuarioId,
                    ExtractPermissionClaims(),
                    requestId,
                    cancellationToken);

                return result.success ? Ok(result) : StatusCode(ResolveStatusCode(result), result);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var error = _errorFactory.FromDescriptor<ConsultaPreviaRadicadoRelacionResponseDto>(
                    ConsultaPreviaRadicadoErrorCatalog.Get(ConsultaPreviaRadicadoErrorCatalog.RelacionUnexpected),
                    "unexpected",
                    requestId,
                    ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        private static int ResolveStatusCode<T>(AppResponses<T> response)
        {
            var code = response.errors?
                .OfType<MiApp.DTOs.DTOs.Errors.AppError>()
                .Select(error => error.Code)
                .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

            return ConsultaPreviaRadicadoErrorResponseFactory.ResolveHttpStatus(code);
        }

        private BaseClaimsResult<T> ValidateBaseClaims<T>(string requestId)
        {
            var alias = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!alias.Success || string.IsNullOrWhiteSpace(alias.ClaimValue))
            {
                return new BaseClaimsResult<T>
                {
                    Error = _errorFactory.Validation<T>(
                        ConsultaPreviaRadicadoErrorCatalog.RelacionClaimRequired,
                        "defaulalias",
                        requestId)
                };
            }

            var usuario = _claimValidationService.ValidateClaim<string>("usuarioid");
            if (!usuario.Success || string.IsNullOrWhiteSpace(usuario.ClaimValue)
                || !long.TryParse(usuario.ClaimValue, out var usuarioId))
            {
                return new BaseClaimsResult<T>
                {
                    Error = _errorFactory.Validation<T>(
                        ConsultaPreviaRadicadoErrorCatalog.RelacionClaimRequired,
                        "usuarioid",
                        requestId)
                };
            }

            return new BaseClaimsResult<T>
            {
                DefaultDbAlias = alias.ClaimValue,
                UsuarioId = usuarioId
            };
        }

        private IReadOnlyCollection<string> ExtractPermissionClaims()
        {
            return User?.Claims?
                .SelectMany(claim => new[] { claim.Type, claim.Value })
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray() ?? [];
        }

        private sealed class BaseClaimsResult<T>
        {
            public string? DefaultDbAlias { get; set; }
            public long UsuarioId { get; set; }
            public AppResponses<T>? Error { get; set; }
        }
    }
}
