using MiApp.DTOs.DTOs.GestorDocumental.Documentos.VisualizacionDocumento;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.Documentos.VisualizacionDocumento;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.GestorDocumental.Documentos
{
    [Route("api/gestor-documental/documentos/visualizacion")]
    [ApiController]
    [Authorize]
    public sealed class VisualizacionDocumentoController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IVisualizacionDocumentoService _service;

        public VisualizacionDocumentoController(
            IClaimValidationService claimValidationService,
            IVisualizacionDocumentoService service)
        {
            _claimValidationService = claimValidationService;
            _service = service;
        }

        [HttpPost("resolve")]
        public async Task<ActionResult<AppResponses<VisualizacionDocumentoResponseDto?>>> Resolve([FromBody] VisualizacionDocumentoRequestDto request)
        {
            try
            {
                var aliasValidation = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!aliasValidation.Success || string.IsNullOrWhiteSpace(aliasValidation.ClaimValue))
                {
                    return BadRequest(aliasValidation.Response);
                }

                var userValidation = _claimValidationService.ValidateClaim<string>("usuarioid");
                if (!userValidation.Success || string.IsNullOrWhiteSpace(userValidation.ClaimValue))
                {
                    return BadRequest(userValidation.Response);
                }

                if (!int.TryParse(userValidation.ClaimValue, out var usuarioId) || usuarioId <= 0)
                {
                    return BadRequest(Validation("usuarioid", "Claim invalido: usuarioid"));
                }

                var response = await _service.ResolveAsync(request, aliasValidation.ClaimValue, usuarioId);
                if (!response.success)
                {
                    return BadRequest(response);
                }
                if (response.data != null && !string.IsNullOrWhiteSpace(response.data.UrlTemporal))
                {
                    response.data = new VisualizacionDocumentoResponseDto
                    {
                        IdDocumento = response.data.IdDocumento,
                        NombreGabinete = response.data.NombreGabinete,
                        FileName = response.data.FileName,
                        ContentType = response.data.ContentType,
                        Origen = response.data.Origen,
                        UrlTemporal = response.data.UrlTemporal,
                        UrlTemporalAbsoluta = BuildAbsoluteUrl(response.data.UrlTemporal),
                        ExpiresAt = response.data.ExpiresAt
                    };
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(Validation("visualizacion", ex.Message));
            }
        }

        [HttpGet("download/{token}")]
        public ActionResult Download([FromRoute] string token)
        {
            try
            {
                var userValidation = _claimValidationService.ValidateClaim<string>("usuarioid");
                if (!userValidation.Success || string.IsNullOrWhiteSpace(userValidation.ClaimValue))
                {
                    return BadRequest(userValidation.Response);
                }

                if (!int.TryParse(userValidation.ClaimValue, out var usuarioId) || usuarioId <= 0)
                {
                    return BadRequest(Validation("usuarioid", "Claim invalido: usuarioid"));
                }

                var found = _service.TryResolveDownload(token, usuarioId, out var filePath, out var contentType, out var fileName);
                if (!found)
                {
                    return NotFound();
                }

                var bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(Validation("visualizacionDownload", ex.Message));
            }
        }

        private static AppResponses<VisualizacionDocumentoResponseDto?> Validation(string field, string message)
        {
            return new AppResponses<VisualizacionDocumentoResponseDto?>
            {
                success = false,
                message = "Error de validacion",
                data = null,
                meta = new AppMeta { Status = "validation" },
                errors =
                [
                    new MiApp.DTOs.DTOs.Errors.AppError
                    {
                        Type = "Validation",
                        Field = field,
                        Message = message
                    }
                ]
            };
        }

        private string BuildAbsoluteUrl(string relativeUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(relativeUrl))
                {
                    return string.Empty;
                }

                if (Uri.TryCreate(relativeUrl, UriKind.Absolute, out var absolute))
                {
                    return absolute.ToString();
                }

                var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
                return $"{baseUrl}{relativeUrl}";
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
