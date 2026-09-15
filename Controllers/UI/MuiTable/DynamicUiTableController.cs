using MiApp.DTOs.DTOs.UI.MuiTable;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using MiApp.Services.Service.UI.MuiTable;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DocuArchi.Api.Controllers.UI.MuiTable
{
    [Route("api/ui/dynamic-table")]
    [ApiController]
    [Authorize]
    public sealed class DynamicUiTableController : Controller
    {
        private readonly IClaimValidationService _claimValidationService;
        private readonly IDynamicUiTableService _dynamicUiTableService;

        public DynamicUiTableController(
            IClaimValidationService claimValidationService,
            IDynamicUiTableService dynamicUiTableService)
        {
            _claimValidationService = claimValidationService;
            _dynamicUiTableService = dynamicUiTableService;
        }

        [HttpPost("query")]
        public async Task<ActionResult<AppResponses<object>>> Query([FromBody] DynamicUiTableQueryRequestDto req)
        {
            var aliasClaim = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!aliasClaim.Success || aliasClaim.ClaimValue == null)
            {
                return BadRequest(aliasClaim.Response);
            }

            req.DefaultDbAlias = aliasClaim.ClaimValue;
            req.UserClaims = ExtractClaims();

            var result = await _dynamicUiTableService.QueryAsync(req);
            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("action")]
        public async Task<ActionResult<AppResponses<object>>> Action([FromBody] ExecuteUiActionRequestDto req)
        {
            var aliasClaim = _claimValidationService.ValidateClaim<string>("defaulalias");
            if (!aliasClaim.Success || aliasClaim.ClaimValue == null)
            {
                return BadRequest(aliasClaim.Response);
            }

            req.DefaultDbAlias = aliasClaim.ClaimValue;
            req.UserClaims = ExtractClaims();

            var result = await _dynamicUiTableService.ExecuteActionAsync(req);
            if (!result.success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("/api/{modulo}/ui/table/query")]
        public async Task<ActionResult<AppResponses<object>>> QueryByModulo(string modulo, [FromBody] DynamicUiTableQueryRequestDto req)
        {
            if (string.IsNullOrWhiteSpace(req.TableId))
            {
                req.TableId = modulo;
            }

            return await Query(req);
        }

        [HttpPost("/api/{modulo}/ui/table/action")]
        public async Task<ActionResult<AppResponses<object>>> ActionByModulo(string modulo, [FromBody] ExecuteUiActionRequestDto req)
        {
            if (string.IsNullOrWhiteSpace(req.TableId))
            {
                req.TableId = modulo;
            }

            return await Action(req);
        }

        private List<string> ExtractClaims()
        {
            var acceptedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "permiso",
                "permission",
                "role",
                ClaimTypes.Role
            };

            return User.Claims
                .Where(c => acceptedTypes.Contains(c.Type))
                .Select(c => c.Value)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
