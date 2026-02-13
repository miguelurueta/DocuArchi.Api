using AutoMapper;
using DocuArchiCore.Abstractions.Security;
using DocuArchiCore.Infrastructure.Security;
using MiApp.DTOs.DTOs.Account;
using MiApp.Repositorio.Account;
using MiApp.Repository.DataAccess;
using MiApp.Repository.Repositorio.Account;
using MiApp.Repository.Repositorio.Autenticacion;
using MiApp.Repository.Repositorio.Autenticacion.Recovery;
using MiApp.Repository.Repositorio.DataAccess;
using MiApp.Repository.Repositorio.Docuarchi.Grupo;
using MiApp.Repository.Repositorio.Docuarchi.Usuario;
using MiApp.Repository.Repositorio.GestorDocumental.usuario;
using MiApp.Repository.Repositorio.Home.Menu;
using MiApp.Repository.Repositorio.Radicador.PlantillaRadicado;
using MiApp.Repository.Repositorio.Radicador.PlantillaValidacion;
using MiApp.Repository.Repositorio.Radicador.Tramite;
using MiApp.Repository.Repositorio.Radicador.Usuario;
using MiApp.Repository.Repositorio.Workflow.Eventos;
using MiApp.Repository.Repositorio.Workflow.Grupo;
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
using MiApp.Services.Service.Home.Menu;
using MiApp.Services.Service.Mapping;
using MiApp.Services.Service.Mapping.Home.Menu;
using MiApp.Services.Service.Radicacion.Inicio;
using MiApp.Services.Service.Radicacion.PlantillaRadicado;
using MiApp.Services.Service.Seguridad.Autorizacion.Configuracion;
using MiApp.Services.Service.Seguridad.Autorizacion.Extensiones;
using MiApp.Services.Service.Seguridad.Autorizacion.Test;
using MiApp.Services.Service.Seguridad.PasswordPolice;
using MiApp.Services.Service.SessionHelper;
using MiApp.Services.Service.Usuario;
using MiApp.Services.Service.Workflow.Inicio;
using MiApp.Services.Service.Workflow.Usuario;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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
// CORS para React
// ===================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("react", p =>
        p.WithOrigins("http://localhost:5173")
         .AllowAnyHeader()
         .AllowAnyMethod());
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
builder.Services.AddScoped<IRaValoresCamposSeleccionPlantillaRadicadoR, RaValoresCamposSeleccionPlantillaRadicadoR>();
builder.Services.AddScoped<ISecondFactorChallengeRepository, SecondFactorChallengeRepository>();
builder.Services.AddScoped<IRecoverTokenConsumedRepository, RecoverTokenConsumedRepository>();
builder.Services.AddScoped <IPasswordWorkflowService, PasswordWorkflowService>();
builder.Services.AddScoped<IRemitenteInternoPasswordService, RemitenteInternoPasswordService>();
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
builder.Services.AddScoped<ISecondFactorProvider, EmailSecondFactorProvider>();
builder.Services.AddScoped<IEmailSender, EmailSenderStub>();
builder.Services.AddScoped<IAuthOrchestrator, AuthOrchestrator>();
builder.Services.AddScoped<IPasswordPolicyValidator, PasswordPolicyValidator>();
builder.Services.AddScoped<IPermissionTestService, PermissionTestService>();



// ===================================================
// Infrastructure (Security + Session)
// ===================================================
builder.Services.Configure<PermissionTestSettings>(
    builder.Configuration.GetSection("PermissionTest"));
builder.Services.AddScoped<ITokenIssuer, TokenIssuer>();
builder.Services.AddScoped<IIpHelper, IpHelperL>();

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
app.UseCors("react");
app.UseAuthentication(); // 👈 agregado
app.UseAuthorization();
app.UseSession();
app.UseAuthorization();
app.MapControllers();
app.Run();



