using AutoMapper;
using DocuArchi.Api.Infrastructure.Security;
using DocuArchiCore.Abstractions.Security;
using MiApp.DTOs.DTOs.Account;
using MiApp.Repositorio.Account;
using MiApp.Repository.DataAccess;
using MiApp.Repository.Repositorio.Account;
using MiApp.Repository.Repositorio.Autenticacion;
using MiApp.Repository.Repositorio.Autenticacion.Recovery;
using MiApp.Repository.Repositorio.DataAccess;
using MiApp.Repository.Repositorio.Docuarchi.Grupo;
using MiApp.Repository.Repositorio.Docuarchi.Usuario;
using MiApp.Repository.Repositorio.GestorDocumental.Sede;
using MiApp.Repository.Repositorio.GestorDocumental.usuario;
using MiApp.Repository.Repositorio.Home.Menu;
using MiApp.Repository.Repositorio.Configuracion;
using MiApp.Repository.Repositorio.Radicador.PlantillaRadicado;
using MiApp.Repository.Repositorio.Radicador.PlantillaValidacion;
using MiApp.Repository.Repositorio.Radicador.Configuracion;
using MiApp.Repository.Repositorio.Radicador.Tramite;
using MiApp.Repository.Repositorio.Radicador.Usuario;
using MiApp.Repository.Repositorio.UI.MuiTable;
using MiApp.Repository.Repositorio.Workflow.Eventos;
using MiApp.Repository.Repositorio.Workflow.Flujo;
using MiApp.Repository.Repositorio.Workflow.Grupo;
using MiApp.Repository.Repositorio.Workflow.BandejaCorrespondencia;
using MiApp.Repository.Repositorio.Workflow.RutaTrabajo;
using MiApp.Repository.Repositorio.Workflow.usuario;
using MiApp.Repository.Repositorio.Workflow.Usuario;
using MiApp.Services.Service.Account;
using MiApp.Services.Service.Autenticacion;
using MiApp.Services.Service.Autenticacion.Providers;
using MiApp.Services.Service.Autenticacion.Recovery;
using MiApp.Services.Service.Autenticacion.SecondFactor;
using MiApp.Services.Service.Crypto;
using MiApp.Services.Service.DateHelper;
using MiApp.Services.Service.Docuarchi.Inicio;
using MiApp.Services.Service.Docuarchi.Usuario;
using MiApp.Services.Service.General;
using MiApp.Services.Service.GestorDocumental.Inicio;
using MiApp.Services.Service.GestorDocumental.Usuario;
using MiApp.Services.Service.Home.Menu;
using MiApp.Services.Service.Mapping;
using MiApp.Services.Service.Mapping.Home.Menu;
using MiApp.Services.Service.Radicacion.Inicio;
using MiApp.Services.Service.Radicacion.RelacionCamposRutaWorklflow;
using MiApp.Services.Service.Radicacion.Configuracion;
using MiApp.Services.Service.Radicacion.PlantillaRadicado;
using MiApp.Services.Service.Radicacion.PlantillaValidacion;
using MiApp.Services.Service.Radicacion.Tramite;
using MiApp.Services.Service.Seguridad.Autorizacion.Configuracion;
using MiApp.Services.Service.Seguridad.Autorizacion.CurrentClaim;
using MiApp.Services.Service.Seguridad.Autorizacion.Extensiones;
using MiApp.Services.Service.Seguridad.Autorizacion.Test;
using MiApp.Services.Service.Seguridad.PasswordPolice;
using MiApp.Services.Service.SessionHelper;
using MiApp.Services.Service.UI.MuiTable;
using MiApp.Services.Service.Usuario;
using MiApp.Services.Service.Workflow.BandejaCorrespondencia;
using MiApp.Services.Service.Workflow.Inicio;
using MiApp.Services.Service.Workflow.RutaTrabajo;
using MiApp.Services.Service.Workflow.Usuario;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

// ===================================================
// Controllers + JSON
// ===================================================
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = null);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ===================================================
// AutoMapper
// ===================================================
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperProfile>());
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<RaMenuPrincipalProfile>());

// ===================================================
// CORS para React y otros clientes
// ===================================================
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("react", p =>
    {
        if (allowedOrigins == null || allowedOrigins.Length == 0)
        {
            // Si no hay orígenes configurados, no permitimos ninguno
            p.WithOrigins(Array.Empty<string>())
             .AllowAnyHeader()
             .AllowAnyMethod();
        }
        else
        {
            p.WithOrigins(allowedOrigins)
             .AllowAnyHeader()
             .AllowAnyMethod();
        }
    });
});
// ===================================================
// data Access
// ===================================================
builder.Services.AddScoped<IDapperCrudEngine, DapperCrudEngine>();
builder.Services.AddScoped<IEntidadBuilder, EntidadBuilder>();
builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactoryImpl>();

// ===================================================
// Repositories (R)
// ===================================================
builder.Services.AddScoped<IEmpresaGestionDocumentalR, EmpresaGestionDocumentalR>();
builder.Services.AddScoped<IGestorModuloR, GestorModuloR>();
builder.Services.AddScoped<IMenuR, MenuR>();
builder.Services.AddScoped<IUsuariosDAR, UsuariosDAR>();
builder.Services.AddScoped<IUsuariosDAL, UsuariosDAL>();
builder.Services.AddScoped<ILogUsuarioR, LogUsuarioR>();
builder.Services.AddScoped<IUsuarioWorkflowR, UsuarioWorkflowR>();
builder.Services.AddScoped<IPermisosUsuarioWorkflowR, PermisosUsuarioWorkflowR>();
builder.Services.AddScoped<IIntervaloAlarmasUsuarioR, IntervaloAlarmasUsuarioR>();
builder.Services.AddScoped<IGruposWorkflowR, GruposWorkflowR>();
builder.Services.AddScoped<IScriptActividadesR, ScriptActividadesR>();
builder.Services.AddScoped<IDetallePlantillaRadicadoR, DetallePlantillaRadicadoR>();
builder.Services.AddScoped<IRaRadTipoRadicacionR, RaRadTipoRadicacionR>();
builder.Services.AddScoped<IRaRadEstadosModuloRadicacionR, RaRadEstadosModuloRadicacionR>();
builder.Services.AddScoped<IRaScriptActividadesR, RaScriptActividadesR>();
builder.Services.AddScoped<ICamposPlantillaValidacionR, CamposPlantillaValidacionR>();
builder.Services.AddScoped<IPlantillaValidacionR, PlantillaValidacionR>();
builder.Services.AddScoped<ISystemPlantillaRadicadoR, SystemPlantillaRadicadoR>();
builder.Services.AddScoped<IRelacionUsuGrupR, RelacionUsuGrupR>();
builder.Services.AddScoped<IPerfilarUsuarioRadicadorR, PerfilarUsuarioRadicadorR>();
builder.Services.AddScoped<IRemitDestInternoPerfilProduccionR, RemitDestInternoPerfilProduccionR>();
builder.Services.AddScoped<IWfPerfilDiagramadorR, WfPerfilDiagramadorR>();
builder.Services.AddScoped<IPermisosPlantillaR, PermisosPlantillaR>();
builder.Services.AddScoped<ITipoDocEntranteR, TipoDocEntranteR>();
builder.Services.AddScoped<IRemitDestInternoR, RemitDestInternoR>();
builder.Services.AddScoped<ILogUsuarioDR, LogUsuarioDR>();
builder.Services.AddScoped<ISedeEmpresaR, SedeEmpresaR>();
builder.Services.AddScoped<IRaValoresCamposSeleccionPlantillaRadicadoR, RaValoresCamposSeleccionPlantillaRadicadoR>();
builder.Services.AddScoped<ISecondFactorChallengeRepository, SecondFactorChallengeRepository>();
builder.Services.AddScoped<IRecoverTokenConsumedRepository, RecoverTokenConsumedRepository>();
builder.Services.AddScoped <IPasswordWorkflowService, PasswordWorkflowService>();
builder.Services.AddScoped<IRemitenteInternoPasswordService, RemitenteInternoPasswordService>();
builder.Services.AddScoped<IUsuarioCaracterizacionRepository,  UsuarioCaracterizacionRepository>();
builder.Services.AddScoped<IFlujosRelacionadosTramiteRepository, FlujosRelacionadosTramiteRepository>();
builder.Services.AddScoped<IRaRestriRelacionTramiteR, RaRestriRelacionTramiteR>();
builder.Services.AddScoped<ITotalDiasVencimientoTramiteRepository, TotalDiasVencimientoTramiteRepository>();
builder.Services.AddScoped<IListaDiasFeriadosTramiteRepository, ListaDiasFeriadosTramiteRepository>();
builder.Services.AddScoped<IListaRadicadosPendientesRepository, ListaRadicadosPendientesRepository>();
builder.Services.AddScoped<IRegistroPlantillaBuilder, RegistroPlantillaBuilder>();
builder.Services.AddScoped<IRegistrarRadicacionEntranteRepository, RegistrarRadicacionEntranteRepository>();
builder.Services.AddScoped<IValidaDimensionCamposRepository, ValidaDimensionCamposRepository>();
builder.Services.AddScoped<IValidaTipoCamposRepository, ValidaTipoCamposRepository>();
builder.Services.AddScoped<IValidaCamposDinamicosUnicosRadicacionRepository, ValidaCamposDinamicosUnicosRadicacionRepository>();
builder.Services.AddScoped<IRemitDestInternoR, RemitDestInternoR>();
builder.Services.AddScoped<ICamposDinamicosPlantillaRepository, CamposDinamicosPlantillaRepository>();
builder.Services.AddScoped<ISolicitaListaEstructuraConfiguracionPlantillaRadicacionRepository, SolicitaListaEstructuraConfiguracionPlantillaRadicacionRepository>();
builder.Services.AddScoped<ISolicitaAutoCompleteTokenRadicadoRepository, SolicitaAutoCompleteTokenRadicadoRepository>();
builder.Services.AddScoped<ISolicitaAutoCompleteTokenExpedienteRadicadoRepository, SolicitaAutoCompleteTokenExpedienteRadicadoRepository>();
builder.Services.AddScoped<IUiTableConfigRepository, UiTableConfigRepository>();
builder.Services.AddScoped<IConfiguracionPlantillaRepository, ConfiguracionPlantillaRepository>();
builder.Services.AddScoped<IRelacionCamposRutaWorklflowRepository, RelacionCamposRutaWorklflowRepository>();
builder.Services.AddScoped<ISolicitaExistenciaRadicadoRutaWorkflowRepository, SolicitaExistenciaRadicadoRutaWorkflowRepository>();
builder.Services.AddScoped<ISolicitaEstructuraRutaWorkflowRepository, SolicitaEstructuraRutaWorkflowRepository>();
builder.Services.AddScoped<ISolicitaEstructuraConfiguracionListadoRutaRepository, SolicitaEstructuraConfiguracionListadoRutaRepository>();
builder.Services.AddScoped<ISolicitaCamposListaGestionCorrespondenciaRepository, SolicitaCamposListaGestionCorrespondenciaRepository>();
builder.Services.AddScoped<IRegistroRadicadoTareaWorkflowRepository, RegistroRadicadoTareaWorkflowRepository>();
builder.Services.AddScoped<ISolicitaDatosActividadInicioFlujoRepository, SolicitaDatosActividadInicioFlujoRepository>();




// ===================================================
// Services (L)
// ===================================================
builder.Services.AddScoped<IInicioSesionL, InicioSesionL>();
builder.Services.AddScoped<IGenerycService, GenerycService>();
builder.Services.AddScoped<ICryptoHelper, CryptoHelper>();
builder.Services.AddScoped<IDateHelper, DateHelper>();
builder.Services.AddScoped<IInicioModuloDocuarchiL, InicioModuloDocuarchiL>();
builder.Services.AddScoped<IInicioModuloRadicacionL, InicioModuloRadicacionL>();
builder.Services.AddScoped<IInicioModuloGestorL, InicioModuloGestorL>();
builder.Services.AddScoped<IIncioModuloWorkflowL, IncioModuloWorkflowL>();
builder.Services.AddScoped<IMenuL, MenuL>();
builder.Services.AddScoped<IUsuarioWorkflowL, UsuarioWorkflowL>();
builder.Services.AddScoped<IPlantillaRadicacionL, PlantillaRadicacionL>();
builder.Services.AddScoped<IRemitDestInternoL, RemitDestInternoL>();
builder.Services.AddScoped<ISesionActualCleaner, SesionActualCleaner>();
builder.Services.AddScoped<ISessionHelperService, SessionHelperService>();
builder.Services.AddScoped<ISecondFactorProvider, EmailSecondFactorProvider>();
builder.Services.AddScoped<IAutenticacionApplicationService, AutenticacionApplicationService>();
builder.Services.AddScoped<ISecondFactorService, SecondFactorService>();
builder.Services.AddScoped<IEmailSender, EmailSenderStub>();
builder.Services.AddScoped<IAuthOrchestrator, AuthOrchestrator>();
builder.Services.AddScoped<IPasswordPolicyValidator, PasswordPolicyValidator>();
builder.Services.AddScoped<IPermissionTestService, PermissionTestService>();
builder.Services.AddScoped<IPlantillaValidacionL, PlantillaValidacionL>();
builder.Services.AddScoped<IUsuarioCaracterizacionService,  UsuarioCaracterizacionService>();
builder.Services.AddScoped<IFlujosRelacionadosTramiteService, FlujosRelacionadosTramiteService>();
builder.Services.AddScoped<IRelacionTipoRestriccionService, RelacionTipoRestriccionService>();
builder.Services.AddScoped<ITotalDiasVencimientoTramiteService, TotalDiasVencimientoTramiteService>();
builder.Services.AddScoped<IListaDiasFeriadosTramiteService, ListaDiasFeriadosTramiteService>();
builder.Services.AddScoped<IAutoCompleteDestinatarioRestriccionService, AutoCompleteDestinatarioRestriccionService>();
builder.Services.AddScoped<ICamposDinamicosPlantillaService, CamposDinamicosPlantillaService>();
builder.Services.AddScoped<ISolicitaAutoCompleteTokenRadicadoService, SolicitaAutoCompleteTokenRadicadoService>();
builder.Services.AddScoped<ISolicitaAutoCompleteTokenExpedienteRadicadoService, SolicitaAutoCompleteTokenExpedienteRadicadoService>();
builder.Services.AddScoped<IFechaLimiteRespuestaService, FechaLimiteRespuestaService>();
builder.Services.AddScoped<IListaRadicadosPendientesService, ListaRadicadosPendientesService>();
builder.Services.AddScoped<ISolicitaCuerpoRadicadoService, SolicitaCuerpoRadicadoService>();
builder.Services.AddScoped<IRegistrarRadicacionEntranteService, RegistrarRadicacionEntranteService>();
builder.Services.AddScoped<ISolicitaParametrosRadicadosService, SolicitaParametrosRadicadosService>();
builder.Services.AddScoped<IValidaCamposObligatoriosService, ValidaCamposObligatoriosService>();
builder.Services.AddScoped<IValidaDimensionCamposService, ValidaDimensionCamposService>();
builder.Services.AddScoped<IValidaTipoCamposService, ValidaTipoCamposService>();
builder.Services.AddScoped<IValidaCamposDinamicosUnicosRadicacionService, ValidaCamposDinamicosUnicosRadicacionService>();
builder.Services.AddScoped<IValidaCamposRadicacionService, ValidaCamposRadicacionService>();
builder.Services.AddScoped<IValidarRadicacionEntranteService, ValidarRadicacionEntranteService>();
builder.Services.AddScoped<IFlujoInicialRadicacionService, FlujoInicialRadicacionService>();
builder.Services.AddScoped<IAsingacionValoresDatosRadicadoRutaWorklflow, AsingacionValoresDatosRadicadoRutaWorklflow>();
builder.Services.AddScoped<IValidaDatosRadicacionTareaWorkflowService, ValidaDatosRadicacionTareaWorkflowService>();
builder.Services.AddScoped<IValidaPreRegistroWorkflowService, ValidaPreRegistroWorkflowService>();
builder.Services.AddScoped<IWorkflowDynamicUiColumnBuilder, WorkflowDynamicUiColumnBuilder>();
builder.Services.AddScoped<IDynamicUiTableBuilder, DynamicUiTableBuilder>();
builder.Services.AddScoped<IDynamicUiTableService, DynamicUiTableService>();
builder.Services.AddScoped<IDynamicUiTableHandler, DefaultDynamicUiTableHandler>();
builder.Services.AddScoped<IConfiguracionPlantillaService, ConfiguracionPlantillaService>();
builder.Services.AddScoped<IRelacionCamposRutaWorklflowService, RelacionCamposRutaWorklflowService>();
builder.Services.AddScoped<ISolicitaExistenciaRadicadoRutaWorkflowService, SolicitaExistenciaRadicadoRutaWorkflowService>();
builder.Services.AddScoped<ISolicitaEstructuraRutaWorkflowService, SolicitaEstructuraRutaWorkflowService>();
// ===================================================
// Infrastructure (Security + Session)
// ===================================================
builder.Services.Configure<PermissionTestSettings>(
builder.Configuration.GetSection("PermissionTest"));
builder.Services.AddScoped<ITokenIssuer, TokenIssuer>();
builder.Services.AddScoped<IIpHelper, IpHelperL>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();  //CAMBIADO:     Retorna el sistema de autenticación actual (JWT o ASP.NET Session) según el contexto
builder.Services.AddScoped<IClaimValidationService, ClaimValidationService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<SesionActual>();

builder.Services.AddScoped<ISesionActual>(sp => sp.GetRequiredService<SesionActual>());
builder.Services.AddScoped<ISesionGeneral>(sp => sp.GetRequiredService<SesionActual>());
builder.Services.AddScoped<ISesionDocuArchi>(sp => sp.GetRequiredService<SesionActual>());
builder.Services.AddScoped<ISesionGestionDocumental>(sp => sp.GetRequiredService<SesionActual>());
builder.Services.AddScoped<ISesionRadicacion>(sp => sp.GetRequiredService<SesionActual>());
builder.Services.AddScoped<ISesionWorkflow>(sp => sp.GetRequiredService<SesionActual>());

// ===================================================
// ASP.NET Session
// ===================================================
var sessionConfig = new SessionConfigDTO();
builder.Configuration.GetSection("SessionConfig").Bind(sessionConfig);
if (sessionConfig.IdleTimeoutMinutes <= 0)
    sessionConfig.IdleTimeoutMinutes = 20;

var dataProtectionKeysPath = Path.Combine(builder.Environment.ContentRootPath, ".tmp", "dataprotection-keys");
Directory.CreateDirectory(dataProtectionKeysPath);
builder.Services
    .AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
    .SetApplicationName("DocuArchi.Api");

builder.Services.AddSingleton(sessionConfig);
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(sessionConfig.IdleTimeoutMinutes);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
    o.Cookie.SameSite = SameSiteMode.Lax;
});
// ===================================================
// ASP.NET JWT Authentication
// ===================================================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
        ),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Permitir preflight sin token
            if (context.HttpContext.Request.Method == "OPTIONS")
            {
                context.NoResult();
            }
            return Task.CompletedTask;
        }
    };
});


builder.Services.AddDocuArchiSecurity();
builder.Services.AddAuthorization();
builder.Services.AddScoped<ITokenService, TokenService>();

// ===================================================
// Build pipeline
// ===================================================
var app = builder.Build();




app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseRouting();                // 🔥 NECESARIO

app.UseCors("react");            // 🔥 AQUÍ VA CORS

app.UseSession();                // si usas Session

app.UseAuthentication();         // JWT
app.UseAuthorization();          // Authorization policies

app.MapControllers();
app.Run();
