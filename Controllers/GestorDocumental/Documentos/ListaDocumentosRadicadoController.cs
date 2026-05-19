using System.Security;
using MiApp.DTOs.DTOs.GestorDocumental.Documentos.ListaDocumentosRadicados;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.Documentos;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.GestorDocumental.Documentos
{
    [Route("api/GestorDocumental/Documentos/ListaDocumentosRadicados")]
    [ApiController]
    [Authorize]
    public sealed class ListaDocumentosRadicadoController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IListaDocumentosRadicadoService _service;

        public ListaDocumentosRadicadoController(
            IClaimValidationService claimValidationService,
            IListaDocumentosRadicadoService service)
        {
            _claimValidationService = claimValidationService;
            _service = service;
        }

        [HttpPost("query")]
        public async Task<ActionResult<AppResponses<object>>> Query([FromBody] ListaDocumentosRadicadosTreeQueryRequestDto request)
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
                    throw new SecurityException("Claim invalido: usuarioid");
                }

                var result = await _service.SolicitaListaDocumentosRadicadosTreeAsync(request, usuarioId, aliasValidation.ClaimValue);
                if (!result.success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new AppResponses<object>
                {
                    success = false,
                    message = "Error en query de ListaDocumentosRadicados",
                    data = null!,
                    meta = new AppMeta { Status = "error" },
                    errors =
                    [
                        new MiApp.DTOs.DTOs.Errors.AppError
                        {
                            Type = "Exception",
                            Field = "query",
                            Message = ex.Message
                        }
                    ]
                });
            }
        }

        [HttpPost("action")]
        public async Task<ActionResult<AppResponses<ListaDocumentosRadicadosTreeMutationResultDto?>>> Action([FromBody] ListaDocumentosRadicadosTreeActionRequestDto request)
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
                    throw new SecurityException("Claim invalido: usuarioid");
                }

                var result = await _service.EjecutaAccionListaDocumentosRadicadosTreeAsync(request, usuarioId, aliasValidation.ClaimValue);
                if (!result.success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new AppResponses<ListaDocumentosRadicadosTreeMutationResultDto?>
                {
                    success = false,
                    message = "Error en action de ListaDocumentosRadicados",
                    data = null,
                    meta = new AppMeta { Status = "error" },
                    errors =
                    [
                        new MiApp.DTOs.DTOs.Errors.AppError
                        {
                            Type = "Exception",
                            Field = "action",
                            Message = ex.Message
                        }
                    ]
                });
            }
        }
    }
}
