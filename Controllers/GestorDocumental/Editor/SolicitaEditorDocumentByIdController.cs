using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiApp.DTOs.DTOs.GestorDocumental.Editor;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Models.Models.GestorDocumental.Editor;
using MiApp.Services.Service.GestorDocumental.Editor;
using MiApp.Services.Service.Seguridad;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using System.Threading.Tasks;

namespace DocuArchi.Api.Controllers.GestorDocumental.Editor
{
    //[Authorize]
    [ApiController]
    [Route("api/gestor-documental/editor")]
    public class SolicitaEditorDocumentByIdController : ControllerBase
    {
        private readonly IServiceSolicitaEditorDocumentById _service;
        private readonly IClaimValidationService _claimValidationService;

        public SolicitaEditorDocumentByIdController(
            IServiceSolicitaEditorDocumentById service,
            IClaimValidationService claimValidationService)
        {
            _service = service;
            _claimValidationService = claimValidationService;
        }

        [HttpGet("document/{documentId:long}")]
        public async Task<IActionResult> GetById([FromRoute] long documentId)
        {
            //var claimResult = _claimValidationService.ValidateClaim<RaEditorDocument>("defaulalias");
            //if (!claimResult.Success)
            //{
            //    return BadRequest(claimResult.Response);
            //}

            //var defaultDbAlias = claimResult.ClaimValue!;
            AppResponses<EditorDocumentDetailResponseDto?> result = await _service.SolicitaByIdAsync(documentId, "DA");

            if (result == null || !result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
