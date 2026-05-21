using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.GestorDocumental.Documentos.FirmaElectronica;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.Documentos.FirmaElectronica;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.GestorDocumental.Documentos
{
    [Route("api/gestor-documental/documentos")]
    [ApiController]
    [Authorize]
    public sealed class FirmaElectronicaDocumentoController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IFirmaElectronicaDocumentoService _service;

        public FirmaElectronicaDocumentoController(
            IClaimValidationService claimValidationService,
            IFirmaElectronicaDocumentoService service)
        {
            _claimValidationService = claimValidationService;
            _service = service;
        }

        [HttpGet("{idArchivo:long}/firma-electronica")]
        public async Task<ActionResult<AppResponses<FirmaElectronicaDocumentoResponseDto>>> GetFirmaElectronicaDocumento(
            [FromRoute] long idArchivo,
            [FromQuery] string nombreGabinete)
        {
            try
            {
                var aliasValidation = _claimValidationService.ValidateClaim<string>("defaulalias");
                if (!aliasValidation.Success || string.IsNullOrWhiteSpace(aliasValidation.ClaimValue))
                {
                    return BadRequest(aliasValidation.Response);
                }

                var result = await _service.GetFirmaElectronicaDocumentoAsync(
                    idArchivo,
                    nombreGabinete,
                    aliasValidation.ClaimValue);

                if (!result.success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new AppResponses<FirmaElectronicaDocumentoResponseDto>
                {
                    success = false,
                    message = "Error en validacion de firma electronica",
                    data = new FirmaElectronicaDocumentoResponseDto(),
                    meta = new AppMeta { Status = "error" },
                    errors =
                    [
                        new AppError
                        {
                            Type = "Exception",
                            Field = "firma-electronica",
                            Message = ex.Message
                        }
                    ]
                });
            }
        }
    }
}
