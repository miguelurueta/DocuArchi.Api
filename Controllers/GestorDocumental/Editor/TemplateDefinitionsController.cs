using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.GestorDocumental.Editor;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.Editor;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DocuArchi.Api.Controllers.GestorDocumental.Editor
{
    [Authorize]
    [ApiController]
    [Route("api/gestor-documental/editor/templates")]
    public sealed class TemplateDefinitionsController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IServiceTemplateDefinitions _service;

        public TemplateDefinitionsController(
            IClaimValidationService claimValidationService,
            IServiceTemplateDefinitions service)
        {
            _claimValidationService = claimValidationService;
            _service = service;
        }

        [HttpPost("definitions")]
        public async Task<ActionResult<AppResponses<TemplateDefinitionDto?>>> CreateDefinition([FromBody] CreateTemplateDefinitionDto request)
        {
            var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!validation.Success || validation.ClaimValue == null)
            {
                return BadRequest(validation.Response);
            }

            if (request == null)
            {
                return BadRequest(ValidationDef("request", "Request requerido"));
            }

            if (string.IsNullOrWhiteSpace(request.TemplateCode))
            {
                return BadRequest(ValidationDef("templateCode", "TemplateCode requerido"));
            }

            if (string.IsNullOrWhiteSpace(request.TemplateName))
            {
                return BadRequest(ValidationDef("templateName", "TemplateName requerido"));
            }

            var result = await _service.CreateDefinitionAsync(request, validation.ClaimValue);
            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("versions")]
        public async Task<ActionResult<AppResponses<TemplateVersionDto?>>> CreateVersion([FromBody] CreateTemplateVersionDto request)
        {
            var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!validation.Success || validation.ClaimValue == null)
            {
                return BadRequest(validation.Response);
            }

            if (request == null)
            {
                return BadRequest(ValidationVer("request", "Request requerido"));
            }

            if (string.IsNullOrWhiteSpace(request.TemplateCode))
            {
                return BadRequest(ValidationVer("templateCode", "TemplateCode requerido"));
            }

            if (request.VersionNumber <= 0)
            {
                return BadRequest(ValidationVer("versionNumber", "VersionNumber requerido"));
            }

            if (string.IsNullOrWhiteSpace(request.TemplateHtml))
            {
                return BadRequest(ValidationVer("templateHtml", "TemplateHtml requerido"));
            }

            var result = await _service.CreateVersionAsync(request, validation.ClaimValue);
            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("definitions/{templateCode}")]
        public async Task<ActionResult<AppResponses<TemplateDefinitionDetailDto?>>> GetByCode([FromRoute] string templateCode)
        {
            var validation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!validation.Success || validation.ClaimValue == null)
            {
                return BadRequest(validation.Response);
            }

            if (string.IsNullOrWhiteSpace(templateCode))
            {
                return BadRequest(ValidationDetail("templateCode", "TemplateCode requerido"));
            }

            var result = await _service.GetDefinitionDetailAsync(templateCode, validation.ClaimValue);
            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        private static AppResponses<TemplateDefinitionDto?> ValidationDef(string field, string message)
            => new AppResponses<TemplateDefinitionDto?> { success = false, message = message, data = null, errors = [new AppError { Type = "Validation", Field = field, Message = message }] };

        private static AppResponses<TemplateVersionDto?> ValidationVer(string field, string message)
            => new AppResponses<TemplateVersionDto?> { success = false, message = message, data = null, errors = [new AppError { Type = "Validation", Field = field, Message = message }] };

        private static AppResponses<TemplateDefinitionDetailDto?> ValidationDetail(string field, string message)
            => new AppResponses<TemplateDefinitionDetailDto?> { success = false, message = message, data = null, errors = [new AppError { Type = "Validation", Field = field, Message = message }] };
    }
}
