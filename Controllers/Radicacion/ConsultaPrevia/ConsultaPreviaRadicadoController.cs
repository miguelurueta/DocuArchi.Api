using System.Diagnostics;
using MiApp.Services.Service.Radicacion.ResolucionDocumental.Errors;
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
        private readonly ILogger<ConsultaPreviaRadicadoController>? _logger;

        /// <summary>Compone la frontera autenticada de Consulta Previa y la proyección de errores seguros de lectura.</summary>
        /// <param name="claimValidationService">Validación de los claims existentes sin aceptar contexto físico del request.</param>
        /// <param name="consultaPreviaRadicadoService">Fachada de casos de uso existentes, sin SQL en el controller.</param>
        /// <param name="errorFactory">Catálogo compatible y diagnóstico seguro de lecturas.</param>
        /// <param name="logger">Registro estructurado de errores de frontera con correlación y actor cuando está acreditado.</param>
        public ConsultaPreviaRadicadoController(
            IClaimValidationService claimValidationService,
            IConsultaPreviaRadicadoService consultaPreviaRadicadoService,
            ConsultaPreviaRadicadoErrorResponseFactory errorFactory,
            ILogger<ConsultaPreviaRadicadoController>? logger = null)
        {
            _claimValidationService = claimValidationService;
            _consultaPreviaRadicadoService = consultaPreviaRadicadoService;
            _errorFactory = errorFactory;
            _logger = logger;
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
            var requestId = ErroresResolucionDocumental.NormalizarCorrelacionDocumental(HttpContext?.TraceIdentifier);
            var inicio = Stopwatch.StartNew();
            long? usuarioIdResuelto = null;

            try
            {
                var alias = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!alias.Success || string.IsNullOrWhiteSpace(alias.ClaimValue))
                {
                    return BadRequest(_errorFactory.CrearFalloLecturaConsultaPrevia<ConsultaPreviaRadicadoEstructuraDto>(
                        ConsultaPreviaRadicadoErrorCatalog.ClaimRequired,
                        "defaulalias",
                        requestId, "estructura", logger: _logger, usuarioId: usuarioIdResuelto, duracionMs: inicio.ElapsedMilliseconds));
                }

                var usuario = _claimValidationService.ValidateClaim<string>("usuarioid");
                if (!usuario.Success || string.IsNullOrWhiteSpace(usuario.ClaimValue)
                    || !long.TryParse(usuario.ClaimValue, out var usuarioId))
                {
                    return BadRequest(_errorFactory.CrearFalloLecturaConsultaPrevia<ConsultaPreviaRadicadoEstructuraDto>(
                        ConsultaPreviaRadicadoErrorCatalog.ClaimRequired,
                        "usuarioid",
                        requestId, "estructura", logger: _logger, usuarioId: usuarioIdResuelto, duracionMs: inicio.ElapsedMilliseconds));
                }

                usuarioIdResuelto = usuarioId;
                var result = await _consultaPreviaRadicadoService.GetEstructuraAsync(
                    idPlantilla,
                    tipo,
                    alias.ClaimValue,
                    usuarioId,
                    requestId,
                    cancellationToken);


                result = _errorFactory.CompletarDiagnosticoLecturaConsultaPrevia(result, requestId, "estructura", _logger, usuarioIdResuelto, inicio.ElapsedMilliseconds);
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
                var error = _errorFactory.CrearFalloLecturaConsultaPrevia<ConsultaPreviaRadicadoEstructuraDto>(
                    ConsultaPreviaRadicadoErrorCatalog.Unexpected, "unexpected", requestId, "estructura", ex,
                    logger: _logger, usuarioId: usuarioIdResuelto, duracionMs: inicio.ElapsedMilliseconds);
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
            var requestId = ErroresResolucionDocumental.NormalizarCorrelacionDocumental(HttpContext?.TraceIdentifier);
            var inicio = Stopwatch.StartNew();
            long? usuarioIdResuelto = null;

            try
            {
                if (request == null)
                {
                    return BadRequest(_errorFactory.CrearFalloLecturaConsultaPrevia<DynamicUiTableDto>(
                        ConsultaPreviaRadicadoErrorCatalog.QueryRequestInvalido,
                        "request",
                        requestId, "query", logger: _logger, usuarioId: usuarioIdResuelto, duracionMs: inicio.ElapsedMilliseconds));
                }

                var alias = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!alias.Success || string.IsNullOrWhiteSpace(alias.ClaimValue))
                {
                    return BadRequest(_errorFactory.CrearFalloLecturaConsultaPrevia<DynamicUiTableDto>(
                        ConsultaPreviaRadicadoErrorCatalog.QueryClaimRequired,
                        "defaulalias",
                        requestId, "query", logger: _logger, usuarioId: usuarioIdResuelto, duracionMs: inicio.ElapsedMilliseconds));
                }

                var usuario = _claimValidationService.ValidateClaim<string>("usuarioid");
                if (!usuario.Success || string.IsNullOrWhiteSpace(usuario.ClaimValue)
                    || !long.TryParse(usuario.ClaimValue, out var usuarioId))
                {
                    return BadRequest(_errorFactory.CrearFalloLecturaConsultaPrevia<DynamicUiTableDto>(
                        ConsultaPreviaRadicadoErrorCatalog.QueryClaimRequired,
                        "usuarioid",
                        requestId, "query", logger: _logger, usuarioId: usuarioIdResuelto, duracionMs: inicio.ElapsedMilliseconds));
                }

                usuarioIdResuelto = usuarioId;
                var result = await _consultaPreviaRadicadoService.QueryAsync(
                    request,
                    alias.ClaimValue,
                    usuarioId,
                    requestId,
                    cancellationToken);


                result = _errorFactory.CompletarDiagnosticoLecturaConsultaPrevia(result, requestId, "query", _logger, usuarioIdResuelto, inicio.ElapsedMilliseconds);
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
                var error = _errorFactory.CrearFalloLecturaConsultaPrevia<DynamicUiTableDto>(
                    ConsultaPreviaRadicadoErrorCatalog.QueryUnexpected, "unexpected", requestId, "query", ex,
                    logger: _logger, usuarioId: usuarioIdResuelto, duracionMs: inicio.ElapsedMilliseconds);
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
        /// <returns>Contrato de sugerencias configuradas o error catalogado con diagnóstico seguro.</returns>
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
            var requestId = ErroresResolucionDocumental.NormalizarCorrelacionDocumental(HttpContext?.TraceIdentifier);
            var inicio = Stopwatch.StartNew();
            long? usuarioIdResuelto = null;

            try
            {
                if (request == null)
                {
                    return BadRequest(_errorFactory.CrearFalloLecturaConsultaPrevia<ConsultaPreviaRadicadoAutocompleteResponseDto>(
                        ConsultaPreviaRadicadoErrorCatalog.AutocompleteRequestInvalido,
                        "request",
                        requestId, "autocomplete", logger: _logger, usuarioId: usuarioIdResuelto, duracionMs: inicio.ElapsedMilliseconds));
                }

                var alias = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!alias.Success || string.IsNullOrWhiteSpace(alias.ClaimValue))
                {
                    return BadRequest(_errorFactory.CrearFalloLecturaConsultaPrevia<ConsultaPreviaRadicadoAutocompleteResponseDto>(
                        ConsultaPreviaRadicadoErrorCatalog.AutocompleteClaimRequired,
                        "defaulalias",
                        requestId, "autocomplete", logger: _logger, usuarioId: usuarioIdResuelto, duracionMs: inicio.ElapsedMilliseconds));
                }

                var usuario = _claimValidationService.ValidateClaim<string>("usuarioid");
                if (!usuario.Success || string.IsNullOrWhiteSpace(usuario.ClaimValue)
                    || !long.TryParse(usuario.ClaimValue, out var usuarioId))
                {
                    return BadRequest(_errorFactory.CrearFalloLecturaConsultaPrevia<ConsultaPreviaRadicadoAutocompleteResponseDto>(
                        ConsultaPreviaRadicadoErrorCatalog.AutocompleteClaimRequired,
                        "usuarioid",
                        requestId, "autocomplete", logger: _logger, usuarioId: usuarioIdResuelto, duracionMs: inicio.ElapsedMilliseconds));
                }

                usuarioIdResuelto = usuarioId;
                var result = await _consultaPreviaRadicadoService.GetAutocompleteAsync(
                    idPlantilla,
                    nombreCanonico,
                    request,
                    alias.ClaimValue,
                    usuarioId,
                    requestId,
                    cancellationToken);


                result = _errorFactory.CompletarDiagnosticoLecturaConsultaPrevia(result, requestId, "autocomplete", _logger, usuarioIdResuelto, inicio.ElapsedMilliseconds);
                return result.success ? Ok(result) : StatusCode(ResolveStatusCode(result), result);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var error = _errorFactory.CrearFalloLecturaConsultaPrevia<ConsultaPreviaRadicadoAutocompleteResponseDto>(
                    ConsultaPreviaRadicadoErrorCatalog.AutocompleteUnexpected, "unexpected", requestId, "autocomplete", ex,
                    logger: _logger, usuarioId: usuarioIdResuelto, duracionMs: inicio.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        /// <summary>
        /// Obtiene el detalle vertical de un radicado por su identidad persistida, sin exigir un listado previo.
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
            var requestId = ErroresResolucionDocumental.NormalizarCorrelacionDocumental(HttpContext?.TraceIdentifier);
            var inicio = Stopwatch.StartNew();
            long? usuarioIdResuelto = null;

            try
            {
                var alias = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!alias.Success || string.IsNullOrWhiteSpace(alias.ClaimValue))
                {
                    return BadRequest(_errorFactory.CrearFalloLecturaConsultaPrevia<ConsultaPreviaRadicadoDetalleDto>(
                        ConsultaPreviaRadicadoErrorCatalog.DetalleClaimRequired,
                        "defaulalias",
                        requestId, "detalle", logger: _logger, usuarioId: usuarioIdResuelto, duracionMs: inicio.ElapsedMilliseconds));
                }

                var usuario = _claimValidationService.ValidateClaim<string>("usuarioid");
                if (!usuario.Success || string.IsNullOrWhiteSpace(usuario.ClaimValue)
                    || !long.TryParse(usuario.ClaimValue, out var usuarioId))
                {
                    return BadRequest(_errorFactory.CrearFalloLecturaConsultaPrevia<ConsultaPreviaRadicadoDetalleDto>(
                        ConsultaPreviaRadicadoErrorCatalog.DetalleClaimRequired,
                        "usuarioid",
                        requestId, "detalle", logger: _logger, usuarioId: usuarioIdResuelto, duracionMs: inicio.ElapsedMilliseconds));
                }

                usuarioIdResuelto = usuarioId;
                var result = await _consultaPreviaRadicadoService.GetDetalleAsync(
                    radicado,
                    idPlantilla,
                    tipo,
                    alias.ClaimValue,
                    usuarioId,
                    requestId,
                    cancellationToken);


                result = _errorFactory.CompletarDiagnosticoLecturaConsultaPrevia(result, requestId, "detalle", _logger, usuarioIdResuelto, inicio.ElapsedMilliseconds);
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
                var error = _errorFactory.CrearFalloLecturaConsultaPrevia<ConsultaPreviaRadicadoDetalleDto>(
                    ConsultaPreviaRadicadoErrorCatalog.DetalleUnexpected, "unexpected", requestId, "detalle", ex,
                    logger: _logger, usuarioId: usuarioIdResuelto, duracionMs: inicio.ElapsedMilliseconds);
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
            var requestId = ErroresResolucionDocumental.NormalizarCorrelacionDocumental(HttpContext?.TraceIdentifier);
            var inicio = Stopwatch.StartNew();
            long? usuarioIdResuelto = null;

            try
            {
                if (request == null)
                {
                    return BadRequest(_errorFactory.CrearFalloCopiaConsultaPrevia<ConsultaPreviaRadicadoCopiarDatosResponseDto>(
                        ConsultaPreviaRadicadoErrorCatalog.CopiarRequestInvalido,
                        "request",
                        requestId, logger: _logger, usuarioId: usuarioIdResuelto, duracionMs: inicio.ElapsedMilliseconds));
                }

                var alias = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!alias.Success || string.IsNullOrWhiteSpace(alias.ClaimValue))
                {
                    return BadRequest(_errorFactory.CrearFalloCopiaConsultaPrevia<ConsultaPreviaRadicadoCopiarDatosResponseDto>(
                        ConsultaPreviaRadicadoErrorCatalog.CopiarClaimRequired,
                        "defaulalias",
                        requestId, logger: _logger, usuarioId: usuarioIdResuelto, duracionMs: inicio.ElapsedMilliseconds));
                }

                var usuario = _claimValidationService.ValidateClaim<string>("usuarioid");
                if (!usuario.Success || string.IsNullOrWhiteSpace(usuario.ClaimValue)
                    || !long.TryParse(usuario.ClaimValue, out var usuarioId) || usuarioId <= 0)
                {
                    return BadRequest(_errorFactory.CrearFalloCopiaConsultaPrevia<ConsultaPreviaRadicadoCopiarDatosResponseDto>(
                        ConsultaPreviaRadicadoErrorCatalog.CopiarClaimRequired,
                        "usuarioid",
                        requestId, logger: _logger, usuarioId: usuarioIdResuelto, duracionMs: inicio.ElapsedMilliseconds));
                }

                usuarioIdResuelto = usuarioId;
                var result = await _consultaPreviaRadicadoService.CopiarDatosAsync(
                    request,
                    alias.ClaimValue,
                    usuarioId,
                    requestId,
                    cancellationToken);

                result = _errorFactory.CompletarDiagnosticoCopiaConsultaPrevia(result, requestId, _logger, usuarioIdResuelto, inicio.ElapsedMilliseconds);

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
                var error = _errorFactory.CrearFalloCopiaConsultaPrevia<ConsultaPreviaRadicadoCopiarDatosResponseDto>(
                    ConsultaPreviaRadicadoErrorCatalog.CopiarUnexpected, "unexpected", requestId, ex,
                    logger: _logger, usuarioId: usuarioIdResuelto, duracionMs: inicio.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        /// <summary>
        /// Anexa el radicado destino al expediente asociado al radicado origen.
        /// </summary>
        /// <param name="request">Solicitud pública existente de la acción; el contexto proviene de claims autenticados.</param>
        /// <param name="cancellationToken">Cancelación propagada a preparación y persistencia.</param>
        /// <returns>Resultado HTTP con catálogo y DTO compatibles; errores con diagnóstico seguro en Production.</returns>
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
            var requestId = ErroresResolucionDocumental.NormalizarCorrelacionDocumental(HttpContext?.TraceIdentifier);
            var reloj = Stopwatch.StartNew();
            long? usuarioId = null;

            try
            {
                if (request == null)
                {
                    return BadRequest(_errorFactory.CrearFalloAccionConsultaPrevia<ConsultaPreviaRadicadoAnexarExpedienteResponseDto>(
                        ConsultaPreviaRadicadoErrorCatalog.RelacionRequestInvalido,
                        "request",
                        requestId, "anexar-expediente", logger: _logger, duracionMs: reloj.ElapsedMilliseconds));
                }

                var claims = ValidateBaseClaims<ConsultaPreviaRadicadoAnexarExpedienteResponseDto>(requestId);
                if (claims.Error != null)
                {
                    return BadRequest(_errorFactory.CompletarDiagnosticoAccionConsultaPrevia(claims.Error, requestId, "anexar-expediente", _logger, duracionMs: reloj.ElapsedMilliseconds));
                }

                usuarioId = claims.UsuarioId;
                var result = await _consultaPreviaRadicadoService.AnexarExpedienteAsync(
                    request,
                    claims.DefaultDbAlias!,
                    claims.UsuarioId,
                    ExtractPermissionClaims(),
                    requestId,
                    cancellationToken);

                if (!result.success) result = _errorFactory.CompletarDiagnosticoAccionConsultaPrevia(result, requestId, "anexar-expediente", _logger, usuarioId, reloj.ElapsedMilliseconds);
                return result.success ? Ok(result) : StatusCode(ResolveStatusCode(result), result);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var error = _errorFactory.CrearFalloAccionConsultaPrevia<ConsultaPreviaRadicadoAnexarExpedienteResponseDto>(
                    ConsultaPreviaRadicadoErrorCatalog.RelacionUnexpected,
                    "unexpected",
                    requestId,
                    "anexar-expediente", ex, _logger, usuarioId, reloj.ElapsedMilliseconds);
                return StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }

        /// <summary>
        /// Relaciona dos radicados desde Consulta Previa con modo agrega o remplaza.
        /// </summary>
        /// <param name="request">Solicitud pública existente de la acción; el contexto proviene de claims autenticados.</param>
        /// <param name="cancellationToken">Cancelación propagada a preparación y persistencia.</param>
        /// <returns>Resultado HTTP con catálogo y DTO compatibles; errores con diagnóstico seguro en Production.</returns>
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
            var requestId = ErroresResolucionDocumental.NormalizarCorrelacionDocumental(HttpContext?.TraceIdentifier);
            var reloj = Stopwatch.StartNew();
            long? usuarioId = null;

            try
            {
                if (request == null)
                {
                    return BadRequest(_errorFactory.CrearFalloAccionConsultaPrevia<ConsultaPreviaRadicadoRelacionResponseDto>(
                        ConsultaPreviaRadicadoErrorCatalog.RelacionRequestInvalido,
                        "request",
                        requestId, "relacionar", logger: _logger, duracionMs: reloj.ElapsedMilliseconds));
                }

                var claims = ValidateBaseClaims<ConsultaPreviaRadicadoRelacionResponseDto>(requestId);
                if (claims.Error != null)
                {
                    return BadRequest(_errorFactory.CompletarDiagnosticoAccionConsultaPrevia(claims.Error, requestId, "relacionar", _logger, duracionMs: reloj.ElapsedMilliseconds));
                }

                usuarioId = claims.UsuarioId;
                var result = await _consultaPreviaRadicadoService.RelacionarAsync(
                    request,
                    claims.DefaultDbAlias!,
                    claims.UsuarioId,
                    ExtractPermissionClaims(),
                    requestId,
                    cancellationToken);

                if (!result.success) result = _errorFactory.CompletarDiagnosticoAccionConsultaPrevia(result, requestId, "relacionar", _logger, usuarioId, reloj.ElapsedMilliseconds);
                return result.success ? Ok(result) : StatusCode(ResolveStatusCode(result), result);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var error = _errorFactory.CrearFalloAccionConsultaPrevia<ConsultaPreviaRadicadoRelacionResponseDto>(
                    ConsultaPreviaRadicadoErrorCatalog.RelacionUnexpected,
                    "unexpected",
                    requestId,
                    "relacionar", ex, _logger, usuarioId, reloj.ElapsedMilliseconds);
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
