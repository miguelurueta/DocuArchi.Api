namespace DocuArchi.Api.Controllers.Account
{
    
    using MiApp.DTOs.DTOs.Autenticacion.TestClaim;
    using MiApp.DTOs.DTOs.Errors;
    using MiApp.DTOs.DTOs.Utilidades;
    
    using MiApp.Services.Service.Seguridad.Autorizacion.Configuracion;
    using MiApp.Services.Service.Seguridad.Autorizacion.Test;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;

    [ApiController]
    [Route("api/seguridad")]
    public class PermissionTestController : ControllerBase
    {
        private readonly IPermissionTestService _permissionTestService;
        private readonly PermissionTestSettings _settings;

        public PermissionTestController(
            IPermissionTestService permissionTestService,
            IOptions<PermissionTestSettings> options)
        {
            _permissionTestService = permissionTestService;
            _settings = options.Value;
        }
        [HttpPost("TestUserClaim")]
        public async Task<ActionResult<AppResponses<object>>> TestUserClaim(
       [FromBody] TestPermisosRequestDTO dto)
        {
            try
            {
                // =========================
                // 1️⃣ Protección básica
                // =========================
                var headerSecret = dto.ValidationToken;
                if (string.IsNullOrWhiteSpace(headerSecret) ||
                    headerSecret != _settings.Secret)
                {
                    return Unauthorized(new AppResponses<object>
                    {
                        success = false,
                        message = "Acceso no autorizado al endpoint de validación.",
                        data = null,
                        errors = new[]
                        {
                        new AppError
                        {
                            Type = "Security",
                            Field = "X-Permission-Test-Key",
                            Message = "Header de validación inválido o ausente."
                        }
                    }
                    });
                }

                // =========================
                // 2️⃣ Ejecutar servicio real
                // =========================
                var serviceResult = await _permissionTestService.ExecuteAsync(dto);

                if (!serviceResult.success || serviceResult.data == null)
                {
                    // 🔴 QA CRÍTICO
                    if (serviceResult.errors?
                        .OfType<AppError>()
                        .Any(e => e.Type == "QA") == true)
                    {
                        return StatusCode(500, serviceResult);
                    }

                    // 🟡 Error funcional
                    return BadRequest(serviceResult);
                }

                // =========================
                // 3️⃣ Validar tipo REAL devuelto
                // =========================
                if (serviceResult.data is not TestUserClaimResponseDTO response)
                {
                    return StatusCode(500, new AppResponses<object>
                    {
                        success = false,
                        message = "El formato de la respuesta de permisos no es válido.",
                        data = null,
                        errors = new[]
                        {
                    new AppError
                    {
                        Type = "Contract",
                        Field = "ExecuteAsync",
                        Message = "El servicio no retornó TestUserClaimResponseDTO."
                    }
                }
                    });
                }

                // =========================
                // 4️⃣ Exportación CSV (opcional)
                // =========================
                if (dto.ExportCsv)
                {
                    var csvBytes = PermissionCsvBuilder.Build(response);

                    return File(
                        csvBytes,
                        "text/csv",
                        $"permission-catalog-{DateTime.UtcNow:yyyyMMddHHmmss}.csv"
                    );
                }

                // =========================
                // 5️⃣ Respuesta JSON
                // =========================
                return Ok(new AppResponses<object>
                {
                    success = true,
                    message = "✅ Validación de permisos ejecutada correctamente.",
                    data = response,
                    errors = null
                });
            }
            catch (Exception ex)
            {
                // =========================
                // 6️⃣ Error no controlado
                // =========================
                return StatusCode(500, new AppResponses<object>
                {
                    success = false,
                    message = "❌ Error inesperado al ejecutar la validación de permisos.",
                    data = null,
                    errors = new[]
                    {
                new AppError
                {
                    Type = "Exception",
                    Field = nameof(TestUserClaim),
                    Message = ex.Message
                }
            }
                });
            }
        }
        
    }

}
