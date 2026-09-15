using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.GestorDocumental.ConfiguracionUpload;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.ConfiguracionUpload;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.GestorDocumental.ConfiguracionUpload
{
    [Authorize]
    [Route("api/gestor-documental/configuracion-upload")]
    [ApiController]
    public sealed class SolicitaEstructuraConfiguracionUploadController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IServiceSolicitaEstructuraConfiguracionUpload _service;
        private readonly ILogger<SolicitaEstructuraConfiguracionUploadController> _logger;

        public SolicitaEstructuraConfiguracionUploadController(
            IClaimValidationService claimValidationService,
            IServiceSolicitaEstructuraConfiguracionUpload service,
            ILogger<SolicitaEstructuraConfiguracionUploadController> logger)
        {
            _claimValidationService = claimValidationService;
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(AppResponses<List<ConfiguracionUploadResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<List<ConfiguracionUploadResponseDto>>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AppResponses<List<ConfiguracionUploadResponseDto>>>> Get([FromQuery] string? nameProceso)
        {
            var requestId = HttpContext.TraceIdentifier;
            _logger.LogInformation(
                "ConfiguracionUpload API request received requestId={RequestId} nameProceso={NameProceso} stage={Stage}",
                requestId,
                nameProceso,
                "api.start");

            var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!validation.Success || string.IsNullOrWhiteSpace(validation.ClaimValue))
            {
                _logger.LogWarning(
                    "ConfiguracionUpload API validation failed requestId={RequestId} stage={Stage} field={Field}",
                    requestId,
                    "api.claims",
                    "defaulalias");

                return BadRequest(validation.Response);
            }

            if (string.IsNullOrWhiteSpace(nameProceso))
            {
                _logger.LogWarning(
                    "ConfiguracionUpload API validation failed requestId={RequestId} stage={Stage} field={Field}",
                    requestId,
                    "api.validation",
                    "nameProceso");

                return BadRequest(new AppResponses<List<ConfiguracionUploadResponseDto>>
                {
                    success = false,
                    message = "NameProceso requerido",
                    data = [],
                    meta = new AppMeta
                    {
                        Status = "validation",
                        RequestId = requestId
                    },
                    errors =
                    [
                        new AppError
                        {
                            Type = "Validation",
                            Field = "nameProceso",
                            Message = "NameProceso requerido"
                        }
                    ]
                });
            }

            var normalizedNameProceso = nameProceso.Trim();
            var result = await _service.SolicitaEstructuraConfiguracionUploadNameProcesoAsync(
                normalizedNameProceso,
                validation.ClaimValue.Trim(),
                requestId);

            if (!result.success)
            {
                _logger.LogWarning(
                    "ConfiguracionUpload API result failed requestId={RequestId} nameProceso={NameProceso} stage={Stage} metaStatus={MetaStatus}",
                    requestId,
                    normalizedNameProceso,
                    "api.result",
                    result.meta?.Status);

                return BadRequest(result);
            }

            _logger.LogInformation(
                "ConfiguracionUpload API result success requestId={RequestId} nameProceso={NameProceso} stage={Stage} metaStatus={MetaStatus} rows={Rows}",
                requestId,
                normalizedNameProceso,
                "api.result",
                result.meta?.Status,
                result.data?.Count ?? 0);

            return Ok(result);
        }
    }
}
