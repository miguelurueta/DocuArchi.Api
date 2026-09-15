using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.Radicacion.Tramite;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.Radicacion.Tramite;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace DocuArchi.Api.Controllers.Radicacion.Tramite
{
    [Route("api/radicacion/rotulos")]
    [ApiController]
    [Authorize]
    public sealed class RotuloRadicacionController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IRotuloRadicacionService _rotuloService;

        public RotuloRadicacionController(
            IClaimValidationService claimValidationService,
            IRotuloRadicacionService rotuloService)
        {
            _claimValidationService = claimValidationService;
            _rotuloService = rotuloService;
        }

        /// <summary>
        /// Genera el PDF del rotulo de radicacion para impresion desde frontend.
        /// </summary>
        /// <param name="idEstadoRadicado">Identificador del estado de radicacion.</param>
        /// <returns>Archivo PDF o error funcional JSON.</returns>
        [HttpGet("{idEstadoRadicado:long}/pdf")]
        [Produces("application/pdf", "application/json")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<RotuloRadicacionPdfResultDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponses<RotuloRadicacionPdfResultDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(AppResponses<RotuloRadicacionPdfResultDto>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(AppResponses<RotuloRadicacionPdfResultDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ObtenerPdf(long idEstadoRadicado)
        {
            var requestId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");
            try
            {
                var validationAlias = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!validationAlias.Success || string.IsNullOrWhiteSpace(validationAlias.ClaimValue))
                {
                    return BadRequest(validationAlias.Response);
                }

                var validationUsuario = _claimValidationService.ValidateClaim<string>("usuarioid");
                if (!validationUsuario.Success || string.IsNullOrWhiteSpace(validationUsuario.ClaimValue))
                {
                    return BadRequest(validationUsuario.Response);
                }

                if (!int.TryParse(validationUsuario.ClaimValue, out var idUsuarioGestion) || idUsuarioGestion <= 0)
                {
                    return BadRequest(BuildError(
                        "RADICACION_ROTULO_CLAIM_INVALID",
                        "Claim usuarioid invalido",
                        "No fue posible identificar el usuario autenticado.",
                        requestId,
                        "usuarioid"));
                }

                var result = await _rotuloService.GeneraPdf(
                    idEstadoRadicado,
                    idUsuarioGestion,
                    validationAlias.ClaimValue,
                    requestId);
                if (!result.success || result.data == null)
                {
                    return StatusCode(ResolveStatus(result), result);
                }

                Response.Headers.CacheControl = "no-store";
                Response.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline")
                {
                    FileNameStar = result.data.FileName
                }.ToString();

                return File(result.data.PdfBytes, result.data.ContentType);
            }
            catch
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    BuildError(
                        "RADICACION_ROTULO_UNEXPECTED",
                        "Error inesperado al generar rotulo de radicacion",
                        "No fue posible generar el rotulo del radicado seleccionado.",
                        requestId));
            }
        }

        /// <summary>
        /// Genera el PDF del rotulo de radicacion cuando el consumidor solo conoce el consecutivo/radicado.
        /// </summary>
        /// <param name="consecutivoRadicado">Numero de radicado o consecutivo.</param>
        /// <returns>Archivo PDF o error funcional JSON.</returns>
        [HttpGet("por-radicado/{consecutivoRadicado}/pdf")]
        [Produces("application/pdf", "application/json")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<RotuloRadicacionPdfResultDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AppResponses<RotuloRadicacionPdfResultDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(AppResponses<RotuloRadicacionPdfResultDto>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(AppResponses<RotuloRadicacionPdfResultDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ObtenerPdfPorRadicado(string consecutivoRadicado)
        {
            var requestId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");
            try
            {
                var validationAlias = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!validationAlias.Success || string.IsNullOrWhiteSpace(validationAlias.ClaimValue))
                {
                    return BadRequest(validationAlias.Response);
                }

                var validationUsuario = _claimValidationService.ValidateClaim<string>("usuarioid");
                if (!validationUsuario.Success || string.IsNullOrWhiteSpace(validationUsuario.ClaimValue))
                {
                    return BadRequest(validationUsuario.Response);
                }

                if (!int.TryParse(validationUsuario.ClaimValue, out var idUsuarioGestion) || idUsuarioGestion <= 0)
                {
                    return BadRequest(BuildError(
                        "RADICACION_ROTULO_CLAIM_INVALID",
                        "Claim usuarioid invalido",
                        "No fue posible identificar el usuario autenticado.",
                        requestId,
                        "usuarioid"));
                }

                var result = await _rotuloService.GeneraPdfPorConsecutivo(
                    consecutivoRadicado,
                    idUsuarioGestion,
                    validationAlias.ClaimValue,
                    requestId);
                if (!result.success || result.data == null)
                {
                    return StatusCode(ResolveStatus(result), result);
                }

                Response.Headers.CacheControl = "no-store";
                Response.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline")
                {
                    FileNameStar = result.data.FileName
                }.ToString();

                return File(result.data.PdfBytes, result.data.ContentType);
            }
            catch
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    BuildError(
                        "RADICACION_ROTULO_UNEXPECTED",
                        "Error inesperado al generar rotulo de radicacion",
                        "No fue posible generar el rotulo del radicado seleccionado.",
                        requestId));
            }
        }

        private static int ResolveStatus(AppResponses<RotuloRadicacionPdfResultDto> result)
        {
            var code = result.errors?.OfType<AppError>().FirstOrDefault()?.Code ?? string.Empty;
            if (code.Contains("NOT_FOUND", StringComparison.OrdinalIgnoreCase)
                || code.Contains("TEMPLATE_NOT_FOUND", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCodes.Status404NotFound;
            }

            if (code.Contains("PDF_INVALID", StringComparison.OrdinalIgnoreCase)
                || code.Contains("DYNAMIC_DATA", StringComparison.OrdinalIgnoreCase)
                || code.Contains("TOO_LARGE", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCodes.Status409Conflict;
            }

            if (code.Contains("UNEXPECTED", StringComparison.OrdinalIgnoreCase)
                || code.Contains("REPOSITORY_ERROR", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCodes.Status500InternalServerError;
            }

            return StatusCodes.Status400BadRequest;
        }

        private static AppResponses<RotuloRadicacionPdfResultDto> BuildError(
            string code,
            string message,
            string userMessage,
            string requestId,
            string? field = null)
        {
            return new AppResponses<RotuloRadicacionPdfResultDto>
            {
                success = false,
                message = $"{message}. Code={code}; requestId={requestId}",
                data = null!,
                errors =
                [
                    new AppError
                    {
                        Type = "Validation",
                        Code = code,
                        Message = "Error sanitizado",
                        UserMessage = userMessage,
                        Field = field
                    }
                ],
                meta = new AppMeta
                {
                    Status = "ERROR",
                    RequestId = requestId,
                    ResponseVersion = "1.0"
                }
            };
        }
    }
}
