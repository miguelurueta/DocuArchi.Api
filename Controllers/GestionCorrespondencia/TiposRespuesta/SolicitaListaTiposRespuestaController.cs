using MiApp.DTOs.DTOs.Common;
using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestionCorrespondencia.TiposRespuesta;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DocuArchi.Api.Controllers.GestionCorrespondencia.TiposRespuesta
{
    [Authorize]
    [Route("api/gestion-correspondencia/tipos-respuesta")]
    [ApiController]
    public sealed class SolicitaListaTiposRespuestaController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IServiceSolicitaListaTiposRespuesta _service;
        private readonly ILogger<SolicitaListaTiposRespuestaController> _logger;

        public SolicitaListaTiposRespuestaController(
            IClaimValidationService claimValidationService,
            IServiceSolicitaListaTiposRespuesta service,
            ILogger<SolicitaListaTiposRespuestaController> logger)
        {
            _claimValidationService = claimValidationService;
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<AppResponses<List<ResponseDropdownDto>>>> Get()
        {
            var aliasValidation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!aliasValidation.Success || string.IsNullOrWhiteSpace(aliasValidation.ClaimValue))
            {
                return BadRequest(aliasValidation.Response);
            }

            var alias = aliasValidation.ClaimValue.Trim();
            var requestId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");

            _logger.LogInformation(
                "SolicitaListaTiposRespuesta: requestId={RequestId} alias={Alias}",
                requestId,
                alias);

            var result = await _service.SolicitaListaTiposRespuestaAsync(alias, requestId);
            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        private static AppResponses<List<ResponseDropdownDto>> Validation(string field, string message)
        {
            return new AppResponses<List<ResponseDropdownDto>>
            {
                success = false,
                message = message,
                data = [],
                meta = new AppMeta { Status = "error" },
                errors =
                [
                    new AppError
                    {
                        Type = "Validation",
                        Field = field,
                        Message = message
                    }
                ]
            };
        }
    }
}
