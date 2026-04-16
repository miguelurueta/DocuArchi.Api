using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Models.Models.GestorDocumental.ConfiguracionUpload;
using MiApp.Services.Service.GestorDocumental.ConfiguracionUpload;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.GestorDocumental.ConfiguracionUpload
{
    [Route("api/gestor-documental/configuracion-upload")]
    [ApiController]
    public sealed class SolicitaEstructuraConfiguracionUploadController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IServiceSolicitaEstructuraConfiguracionUpload _service;

        public SolicitaEstructuraConfiguracionUploadController(
            IClaimValidationService claimValidationService,
            IServiceSolicitaEstructuraConfiguracionUpload service)
        {
            _claimValidationService = claimValidationService;
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<AppResponses<List<RaConfiguracionUploadModel>>>> Get([FromQuery] string nameProceso)
        {
            var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!validation.Success || validation.ClaimValue == null)
            {
                return BadRequest(validation.Response);
            }

            if (string.IsNullOrWhiteSpace(nameProceso))
            {
                return BadRequest(new AppResponses<List<RaConfiguracionUploadModel>>
                {
                    success = false,
                    message = "NameProceso requerido",
                    data = [],
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

            var result = await _service.SolicitaEstructuraConfiguracionUploadNameProcesoAsync(nameProceso, validation.ClaimValue);
            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
