using MiApp.DTOs.DTOs.Common;
using MiApp.DTOs.DTOs.GestorDocumental.TipologiasDocumentales;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.GestorDocumental.TipologiasDocumentales;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.GestorDocumental.TipologiasDocumentales
{
    [Authorize]
    [Route("api/gestor-documental/tipologias-documentales")]
    [ApiController]
    public sealed class SolicitaListaTipologiasDocumentalesController : ControllerBase
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IServiceSolicitaListaTipologiasDocumentales _service;
        private readonly ILogger<SolicitaListaTipologiasDocumentalesController> _logger;

        public SolicitaListaTipologiasDocumentalesController(
            IClaimValidationService claimValidationService,
            IServiceSolicitaListaTipologiasDocumentales service,
            ILogger<SolicitaListaTipologiasDocumentalesController> logger)
        {
            _claimValidationService = claimValidationService;
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(AppResponses<List<ResponseDropdownDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AppResponses<List<ResponseDropdownDto>>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AppResponses<List<ResponseDropdownDto>>>> Get(
            [FromQuery] SolicitaListaTipologiasDocumentalesRequest? request)
        {
            var safeRequest = request ?? new SolicitaListaTipologiasDocumentalesRequest();
            var requestId = HttpContext.TraceIdentifier;
            _logger.LogInformation(
                "TipologiasDocumentales API request received requestId={RequestId} contexto={Contexto} stage={Stage}",
                requestId,
                safeRequest.Contexto,
                "api.start");

            var aliasValidation = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!aliasValidation.Success || string.IsNullOrWhiteSpace(aliasValidation.ClaimValue))
            {
                _logger.LogWarning(
                    "TipologiasDocumentales API validation failed requestId={RequestId} stage={Stage} field={Field}",
                    requestId,
                    "api.claims",
                    "defaulalias");

                return BadRequest(aliasValidation.Response);
            }

            int? usuarioId = null;
            var usuarioValidation = _claimValidationService.ValidateClaim<string>("usuarioid");
            if (usuarioValidation.Success
                && !string.IsNullOrWhiteSpace(usuarioValidation.ClaimValue)
                && int.TryParse(usuarioValidation.ClaimValue, out var parsedUsuarioId)
                && parsedUsuarioId > 0)
            {
                usuarioId = parsedUsuarioId;
            }

            string? workflowDbAlias = null;
            var workflowAliasValidation = _claimValidationService.ValidateClaim<string>("defaulaliaswf");
            if (workflowAliasValidation is not null
                && workflowAliasValidation.Success
                && !string.IsNullOrWhiteSpace(workflowAliasValidation.ClaimValue))
            {
                workflowDbAlias = workflowAliasValidation.ClaimValue.Trim();
            }

            var result = await _service.SolicitaListaTipologiasDocumentalesAsync(
                safeRequest,
                aliasValidation.ClaimValue.Trim(),
                requestId,
                usuarioId,
                workflowDbAlias);

            if (!result.success)
            {
                _logger.LogWarning(
                    "TipologiasDocumentales API result failed requestId={RequestId} usuarioId={UsuarioId} contexto={Contexto} alias={Alias} stage={Stage} metaStatus={MetaStatus}",
                    requestId,
                    usuarioId,
                    safeRequest.Contexto,
                    aliasValidation.ClaimValue,
                    "api.result",
                    result.meta?.Status);

                return BadRequest(result);
            }

            _logger.LogInformation(
                "TipologiasDocumentales API result success requestId={RequestId} usuarioId={UsuarioId} contexto={Contexto} alias={Alias} stage={Stage} metaStatus={MetaStatus} rows={Rows}",
                requestId,
                usuarioId,
                safeRequest.Contexto,
                aliasValidation.ClaimValue,
                "api.result",
                result.meta?.Status,
                result.data?.Count ?? 0);

            return Ok(result);
        }
    }
}
