using DocuArchiCore.Abstractions.Security;
using MiApp.DTOs.DTOs.Autenticacion;
using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.Home.Menu;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Repository.ErrorController;
using MiApp.Repository.Repositorio.Home.Menu;
using MiApp.Services.Service.Home.Menu;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.Account
{
    [Route("api/Menu")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private IMenuR _menuR;
        private ISesionActual _sesionActual;
        private IMenuL menuL;

        public MenuController(IMenuR menuR , ISesionActual sesionActual , IMenuL menuL )
        {
            _menuR = menuR;
            _sesionActual = sesionActual;
            this.menuL = menuL;
        }

        [HttpPost("inicioMenu")]
        public async Task<ActionResult<MiApp.DTOs.DTOs.Utilidades.AppResponses<List<RaMenuPrincipalDto>>>> ServiceSolicitaEstructuraMenuPrincipal()
        {
            try
            {
              var result = await menuL.InicioMenuPrincipal();
                if (!result.success)
                {
                    return BadRequest(result); // ⬅️ CLAVE
                }
                // 🟢 Flujo correcto (login o 2FA)
                return Ok(result);
            }
            catch (Exception ex)
            {
                var errorResponse = new MiApp.DTOs.DTOs.Utilidades.AppResponses<List<RaMenuPrincipalDto>>
                {
                    success = false,
                    message = "Ocurrió un error inesperado durante la verificación del OTP.",
                    data = null,
                    errors = new[]
                    {
                        new AppError
                        {
                            Type = "System",
                            Field = "",
                            Message = ex.Message
                        }
                    }
                };
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }
    }
}
