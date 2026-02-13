using AutoMapper;
using MiApp.DTOs.Account;
using MiApp.DTOs.DTOs.Account;
using MiApp.DTOs.DTOs.Autenticacion;
using MiApp.DTOs.DTOs.Autenticacion.TestClaim;
using MiApp.DTOs.DTOs.Errors;
using MiApp.DTOs.DTOs.Password;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Repositorio.Account;
using MiApp.Repository.ErrorController;
using MiApp.Repository.Repositorio.Account;
using MiApp.Services.Service.Account;
using MiApp.Services.Service.Autenticacion;
using MiApp.Services.Service.Autenticacion.Recovery;
using MiApp.Services.Service.Autenticacion.SecondFactor;
using MiApp.Services.Service.GestorDocumental.Inicio;
using MiApp.Services.Service.Seguridad.Autorizacion.Claims;
using MiApp.Services.Service.Seguridad.Autorizacion.Test;
using MiApp.Services.Service.Usuario;
using Microsoft.AspNetCore.Mvc;

namespace DocuArchi.Api.Controllers.Account
{
    [Route("api/accout")]
    [ApiController]
    
    public class AccountController :ControllerBase
    {
        private readonly IEmpresaGestionDocumentalR empresaGestionDocumentalR;
        private IMapper mapper { get; }
        private IInicioSesionL InicioSesion { get; }
        private  readonly IInicioModuloGestorL _inicioModuloGestorL;
        private IGestorModuloR GestorModuloR { get; }
        public ITokenService TokenService { get; }
        private readonly IRemitDestInternoR _remitDestInternoR;
        private readonly ISecondFactorService _secondFactorService;
        private readonly IAutenticacionApplicationService _autenticacionApplicationService;
        private readonly IAuthOrchestrator _auth;
        private readonly IPermissionTestService _permissionTestService;
        public AccountController(IEmpresaGestionDocumentalR empresaGestionDocumentalR,
            IMapper mapper, IInicioSesionL inicioSesion, IGestorModuloR gestorModuloR, ITokenService TokenService, ISecondFactorService secondFactorService, IAutenticacionApplicationService autenticacionApplicationService, IAuthOrchestrator auth, IRemitDestInternoR remitDestInternoR, IInicioModuloGestorL inicioModuloGestorL, IPermissionTestService permissionTestService)
        {
            this.empresaGestionDocumentalR = empresaGestionDocumentalR;
            this.mapper = mapper;
            InicioSesion = inicioSesion;
            GestorModuloR = gestorModuloR;
            this.TokenService = TokenService;
            _secondFactorService = secondFactorService;
            _autenticacionApplicationService = autenticacionApplicationService;
            _auth = auth;
            _remitDestInternoR = remitDestInternoR;
            _inicioModuloGestorL = inicioModuloGestorL;
            _permissionTestService = permissionTestService;
        }

        [HttpGet("SolicitaEstructuraEmpresa")]
        public async Task<ActionResult<AppResponses<List<ListaEmpresaDTO>>>> SolicitaEstructuraEmpresaGet()
        {
            var result = await empresaGestionDocumentalR.SolicitaEstructuraEmpresa();

            if (result == null || result.Data == null || !result.Data.Any())
            {
                return base.NotFound(new MiApp.DTOs.DTOs.Utilidades.AppResponses<List<ListaEmpresaDTO>>
                {
                    success = false,
                    message = "No se encontraron empresas",
                    data = new List<ListaEmpresaDTO>()
                });
            }

            var lista = mapper.Map<List<ListaEmpresaDTO>>(result.Data);

            return base.Ok(new MiApp.DTOs.DTOs.Utilidades.AppResponses<List<ListaEmpresaDTO>>
            {
                success = true,
                message = "OK",
                data = lista
            });
        }
        [HttpPost("ValidaUserAplicacion")]
        public async Task<ActionResult<AppResponses<RespuestaAutenticacionDTO>>> ValidaUserAplicacion(
           [FromBody] LoginRequestDTO dto)
        {
            var result = await _autenticacionApplicationService.ValidarLogin(dto);
            // 🔴 Error de negocio / validación
            if (!result.success)
            {
                return BadRequest(result); // ⬅️ CLAVE
            }
            // 🟢 Flujo correcto (login o 2FA)
            return Ok(result);
            
        }

        [HttpPost("SolicitaModulosEmpresa")]
        public async Task<ActionResult<AppResponses<List<ModuloDTO>>>> SolicitaModulosEmpresa(
         [FromBody] EmpresaGestionDocumentalDto dto)
        {
            try
            {
                // ===========================
                // 🧩 Validación básica
                // ===========================
                if (dto == null || dto.IdEmpresa <= 0)
                {
                    return base.BadRequest(new MiApp.DTOs.DTOs.Utilidades.AppResponses<List<ModuloDTO>>
                    {
                        success = false,
                        message = "Debe informar una empresa válida.",
                        errors = [],
                        data = new List<ModuloDTO>()
                    });
                }

                // ===========================
                // 🔍 Llamada a repositorio
                // ===========================
                var result = await GestorModuloR.SolicitaModulosEmpresa(dto.IdEmpresa);

                if (result == null)
                {
                    return base.StatusCode(500, new MiApp.DTOs.DTOs.Utilidades.AppResponses<List<ModuloDTO>>
                    {
                        success = false,
                        message = "No se recibió respuesta del repositorio de módulos." + result.Message,
                        errors = [],
                        data = new List<ModuloDTO>()
                    });
                }

                if (!result.Success || result.Data == null || !result.Data.Any())
                {
                    return base.NotFound(new MiApp.DTOs.DTOs.Utilidades.AppResponses<List<ModuloDTO>>
                    {
                        success = false,
                        message = result.Message ?? "No se encontraron módulos para la empresa.",
                        errors = [],
                        data = new List<ModuloDTO>()
                    });
                }

                // ===========================
                // ✅ Respuesta exitosa
                // ===========================
                return base.Ok(new MiApp.DTOs.DTOs.Utilidades.AppResponses<List<ModuloDTO>>
                {
                    success = true,
                    message = result.Message ?? "OK",
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                return base.StatusCode(500, new MiApp.DTOs.DTOs.Utilidades.AppResponses<List<ModuloDTO>>
                {
                    success = false,
                    message = "Error interno al consultar los módulos de la empresa.",
                    errors = [],
                    data = new List<ModuloDTO>()
                });
            }
        }

        [HttpPost("VerificarSegundoFactor")]
        public async Task<ActionResult<AppResponses<List<RespuestaAutenticacionDTO>>>> VerificarSegundoFactor(
          [FromBody] VerificarSegundoFactorDTO dto)
        {
            try
            {
                // ===========================
                // 🧩 Validación básica DTO
                // ===========================
                if (!Guid.TryParse(dto.ChallengeId, out var challengeId))
                {
                    return base.BadRequest(new MiApp.DTOs.DTOs.Utilidades.AppResponses<object>
                    {
                        success = false,
                        message = "ChallengeId inválido.",
                        errors = new[]
                        {
                    new AppError
                    {
                        Type = "Validation",
                        Field = "ChallengeId",
                        Message = "El identificador del challenge no es válido."
                    }
                },
                        data = null
                    });
                }

                // ===========================
                // 🔐 Lógica principal
                // ===========================
                var result = await _secondFactorService.ValidaTokenSegundoFactorL(
                    challengeId,
                    dto.Code,
                    ""
                );

                // ===========================
                // ❌ Error de negocio / validación
                // ===========================
                if (!result.success)
                {
                    // 400 → error controlado
                    return BadRequest(result);
                }
                // ===========================
                // 🔐 Ininicia sesión (login)
                // ===========================

                // ===========================
                // ✅ Éxito
                // ===========================
                return Ok(result);
            }
            catch (Exception ex)
            {
                // ===========================
                // 🔥 Error no controlado
                // ===========================
                // (Aquí puedes loguear ex si quieres)

                return base.StatusCode(StatusCodes.Status500InternalServerError,
                    new MiApp.DTOs.DTOs.Utilidades.AppResponses<object>
                    {
                        success = false,
                        message = "Error interno al verificar el segundo factor.",
                        errors = new[]
                        {
                    new AppError
                    {
                        Type = "System",
                        Field = "",
                        Message = ex.Message
                    }
                        },
                        data = null
                    });
            }
        }

        [HttpPost("recovery/start")]
        public async Task<ActionResult<AppResponses<List<RecuperarPasswordResponseDTO>>>> RecoveryStart([FromBody] RecuperarPasswordRequestDTO dto)
        {
            try
            {
                var result = await _auth.RecoveryStart(dto);

                if (!result.success)
                {
                    return BadRequest(result); // ⬅️ CLAVE
                }

                // 🟢 Flujo correcto (login o 2FA)
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Puedes loguear el error aquí con tu sistema de logging
                // Ejemplo: _logger.LogError(ex, "Error en RecoveryStart");

                var errorResponse = new MiApp.DTOs.DTOs.Utilidades.AppResponses<List<RecuperarPasswordResponseDTO>>
                {
                    success = false,
                    message = "Ocurrió un error inesperado durante la recuperación.",
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
        [HttpPost("recovery/verify-otp")]
        public async Task<ActionResult<AppResponses<List<OptRecoveryPaswResponseDTO>>>> RecoveryVerifyOtp([FromBody] OptRecoveryPaswReguestDTO req)
        {
            try { 
                var result = await _auth.RecoveryPaswVerifyOtp(req);
                if (!result.success)
                {
                    return BadRequest(result); // ⬅️ CLAVE
                }
                // 🟢 Flujo correcto (login o 2FA)
                return Ok(result);
            } catch (Exception ex)
            {
                var errorResponse = new AppResponses<List<RespuestaAutenticacionDTO>>
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


        [HttpPost("recovery/reset-password")]
        public async Task<ActionResult> RecoveryResetPassword([FromBody] PaswordResetDTO dto)
        {
            // Token viene en Authorization: Bearer ...
            try
            {
                var result = await  _auth.RecoveryResetPassword(dto); ;
                if (!result.success)
                {
                    return BadRequest(result); // ⬅️ CLAVE
                }
                // 🟢 Flujo correcto (login o 2FA)
                return Ok(result);
            }
            catch (Exception ex)
            {
                var errorResponse = new AppResponses<List<RespuestaAutenticacionDTO>>
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
