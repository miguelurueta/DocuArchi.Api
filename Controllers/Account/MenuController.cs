using DocuArchiCore.Abstractions.Security;
using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.Home.Menu;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Repository.Repositorio.Home.Menu;
using MiApp.Services.Service.Home.Menu;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;

namespace DocuArchi.Api.Controllers.Account
{
    [Route("api/Menu")]
    [ApiController]
    [Authorize] // 👈 OBLIGATORIO
    public class MenuController : ControllerBase
    {
        private readonly IMenuR _menuR;
        private readonly ISesionActual _sesionActual;
        private readonly IMenuL _menuL;
        private readonly ICurrentUserService _ICurrentUserService;
        public MenuController(
            IMenuR menuR,
            ISesionActual sesionActual,
            IMenuL menuL,
            ICurrentUserService iCurrentUserService)
        {
            _menuR = menuR;
            _sesionActual = sesionActual;
            _menuL = menuL;
            _ICurrentUserService = iCurrentUserService;
        }

        [HttpPost("inicioMenu")]
        public async Task<ActionResult<AppResponses<List<RaMenuPrincipalDto>>>>
            ServiceSolicitaEstructuraMenuPrincipal()
        {
            try
            {
                int idUser = _ICurrentUserService.UserIdInt;
                var result = await _menuL.InicioMenuPrincipal(idUser);
                if (!result.success)
                    return BadRequest(result);
               
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new AppResponses<List<RaMenuPrincipalDto>>
                    {
                        success = false,
                        message = "Error inesperado al cargar menú.",
                        data = null,
                        errors = new[]
                        {
                            new AppError
                            {
                                Type = "System",
                                Field = nameof(ServiceSolicitaEstructuraMenuPrincipal),
                                Message = ex.Message
                            }
                        }
                    });
            }
        }
    }
}


