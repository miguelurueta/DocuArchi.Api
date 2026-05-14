using MiApp.DTOs.DTOs.GestorDocumental.Editor;
using MiApp.Models.Models.GestorDocumental.Editor;
using MiApp.Repository.Repositorio.GestorDocumental.ConfiguracionUpload;
using MiApp.Repository.Repositorio.GestorDocumental.Editor;
using MiApp.Services.Service.GestorDocumental.ConfiguracionUpload;
using MiApp.Services.Service.GestorDocumental.Editor;
using DocuArchi.Api.Controllers.GestorDocumental.ConfiguracionUpload;
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
using MiApp.Repository.Repositorio.GestorDocumental.Editor;
using MiApp.Repository.Repositorio.GestorDocumental.usuario;
using MiApp.Repository.Repositorio.GestionCorrespondencia.Firmas;
using MiApp.Repository.Repositorio.Home.Menu;
using MiApp.Repository.Repositorio.Configuracion;
using MiApp.Repository.Repositorio.Radicador.PlantillaRadicado;
using MiApp.Repository.Repositorio.Radicador.PlantillaValidacion;
using MiApp.Repository.Repositorio.Radicador.Configuracion;
using MiApp.Repository.Repositorio.Radicador.Tramite;
using MiApp.Repository.Repositorio.Radicador.Usuario;
using MiApp.Repository.Repositorio.UI.MuiTable;
using MiApp.Repository.Repositorio.Workflow.BandejaCorrespondencia;
using MiApp.Repository.Repositorio.Workflow.Eventos;
using MiApp.Repository.Repositorio.Workflow.Flujo;
using MiApp.Repository.Repositorio.Workflow.Grupo;
using MiApp.Repository.Repositorio.Workflow.RutaTrabajo;
using MiApp.Repository.Repositorio.GestionCorrespondencia;
using MiApp.Repository.Repositorio.GestionCorrespondencia.GestionRespuesta;
using MiApp.Repository.Repositorio.GestionCorrespondencia.TiposRespuesta;
using MiApp.Repository.Repositorio.GestionCorrespondencia.PlantillaValidacion.SolicitaCorreoElectronicoRemitente;
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
using MiApp.Services.Service.GestionCorrespondencia.PlantillaValidacion.SolicitaCorreoElectronicoRemitente;
using MiApp.Services.Service.GestionCorrespondencia.Firmas;
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
using MiApp.Services.Service.GestorDocumental;
using MiApp.Services.Service.GestionCorrespondencia.GestionRespuesta;
using MiApp.Services.Service.GestionCorrespondencia.TiposRespuesta;
using MiApp.Services.Service.Workflow.Usuario;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using DocuArchi.Api.Infrastructure.Swagger;
using System.IO;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MiApp.Models.Models.GestorDocumental.AlmacenamientoDocumental.StoragePathOptions>(builder.Configuration.GetSection("StoragePaths"));
builder.Services.Configure<MiApp.Models.Models.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload.StorageUploadOptions>(builder.Configuration.GetSection("StorageUpload"));
builder.Services.Configure<MiApp.Models.Models.GestorDocumental.AlmacenamientoDocumental.StorageMetadataOptions>(builder.Configuration.GetSection("StorageMetadata"));
builder.Services.AddMemoryCache();


// ===================================================
  builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.Editor.IServiceFullSaveEditorDocument, MiApp.Services.Service.GestorDocumental.Editor.ServiceFullSaveEditorDocument>();
// Controllers + JSON
// ===================================================
  builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.Editor.IServiceFullSaveEditorDocument, MiApp.Services.Service.GestorDocumental.Editor.ServiceFullSaveEditorDocument>();
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = null);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<UploadTemporalChunkOperationFilter>();

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese: Bearer {token}"
    });
});

// ===================================================
  builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.Editor.IServiceFullSaveEditorDocument, MiApp.Services.Service.GestorDocumental.Editor.ServiceFullSaveEditorDocument>();
// AutoMapper
// ===================================================
  builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.Editor.IServiceFullSaveEditorDocument, MiApp.Services.Service.GestorDocumental.Editor.ServiceFullSaveEditorDocument>();
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperProfile>());
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<RaMenuPrincipalProfile>());

// ===================================================
  builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.Editor.IServiceFullSaveEditorDocument, MiApp.Services.Service.GestorDocumental.Editor.ServiceFullSaveEditorDocument>();
// CORS para React y otros clientes
// ===================================================
  builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.Editor.IServiceFullSaveEditorDocument, MiApp.Services.Service.GestorDocumental.Editor.ServiceFullSaveEditorDocument>();
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
  builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.Editor.IServiceFullSaveEditorDocument, MiApp.Services.Service.GestorDocumental.Editor.ServiceFullSaveEditorDocument>();
// data Access
// ===================================================
  builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.Editor.IServiceFullSaveEditorDocument, MiApp.Services.Service.GestorDocumental.Editor.ServiceFullSaveEditorDocument>();
builder.Services.AddScoped<IDapperCrudEngine, DapperCrudEngine>();
builder.Services.AddScoped<IEntidadBuilder, EntidadBuilder>();
builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactoryImpl>();

// ===================================================
  builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.Editor.IServiceFullSaveEditorDocument, MiApp.Services.Service.GestorDocumental.Editor.ServiceFullSaveEditorDocument>();
// Repositories (R)
// ===================================================
  builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.Editor.IServiceFullSaveEditorDocument, MiApp.Services.Service.GestorDocumental.Editor.ServiceFullSaveEditorDocument>();
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
builder.Services.AddScoped<ISolicitaEstructuraRespuestaIdTareaRepository, SolicitaEstructuraRespuestaIdTareaRepository>();
builder.Services.AddScoped<ISolicitaListaTiposRespuestaRepository, SolicitaListaTiposRespuestaRepository>();
builder.Services.AddScoped<ISolicitaDocumentosAdjuntosRespuestaRadicadoRepository, SolicitaDocumentosAdjuntosRespuestaRadicadoRepository>();
builder.Services.AddScoped<ISolicitaCorreoElectronicoRemitenteRepository, SolicitaCorreoElectronicoRemitenteRepository>();
builder.Services.AddScoped<ISolicitaEstructuraConfiguracionUploadNameProcesoRepository, SolicitaEstructuraConfiguracionUploadNameProcesoRepository>();
builder.Services.AddScoped<IGuardaEditorDocumentRepository, GuardaEditorDocumentRepository>();
builder.Services.AddScoped<ISolicitaEditorContextDefinitionRepository, SolicitaEditorContextDefinitionRepository>();
builder.Services.AddScoped<IGuardaEditorDocumentContextRepository, GuardaEditorDocumentContextRepository>();
builder.Services.AddScoped<IServiceGuardaEditorDocumentContext, ServiceGuardaEditorDocumentContext>();
builder.Services.AddScoped<ISolicitaEstructuraConfiguracionListadoRutaRepository, SolicitaEstructuraConfiguracionListadoRutaRepository>();
builder.Services.AddScoped<ISolicitaCamposListaGestionCorrespondenciaRepository, SolicitaCamposListaGestionCorrespondenciaRepository>();
builder.Services.AddScoped<IWorkflowRouteColumnConfigRepository, WorkflowRouteColumnConfigRepository>();
builder.Services.AddScoped<IWorkflowInboxContextResolverService, WorkflowInboxContextResolverService>();
builder.Services.AddScoped<IWorkflowInboxQueryBuilder, WorkflowInboxQueryBuilder>();
builder.Services.AddScoped<IRegistroRadicadoTareaWorkflowRepository, RegistroRadicadoTareaWorkflowRepository>();
builder.Services.AddScoped<ISolicitaConfiguracionListaUsuarioWorkflowRepository, SolicitaConfiguracionListaUsuarioWorkflowRepository>();
builder.Services.AddScoped<ISolicitaDatosActividadInicioFlujoRepository, SolicitaDatosActividadInicioFlujoRepository>();




// ===================================================
  builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.Editor.IServiceFullSaveEditorDocument, MiApp.Services.Service.GestorDocumental.Editor.ServiceFullSaveEditorDocument>();
// Services (L)
// ===================================================
  builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.Editor.IServiceFullSaveEditorDocument, MiApp.Services.Service.GestorDocumental.Editor.ServiceFullSaveEditorDocument>();
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
builder.Services.AddScoped<ISolicitaEstructuraTipoDocEntranteService, SolicitaEstructuraTipoDocEntranteService>();
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
builder.Services.AddScoped<IWorkflowInboxRepository, WorkflowInboxRepository>();
builder.Services.AddScoped<IWorkflowInboxService, WorkflowInboxService>();
builder.Services.AddScoped<IWorkflowDynamicUiColumnBuilder, WorkflowDynamicUiColumnBuilder>();
builder.Services.AddScoped<IDynamicUiTableBuilder, DynamicUiTableBuilder>();
builder.Services.AddScoped<IDynamicUiTableService, DynamicUiTableService>();
builder.Services.AddScoped<IDynamicUiTableHandler, DefaultDynamicUiTableHandler>();
builder.Services.AddScoped<IConfiguracionPlantillaService, ConfiguracionPlantillaService>();
builder.Services.AddScoped<IRelacionCamposRutaWorklflowService, RelacionCamposRutaWorklflowService>();
builder.Services.AddScoped<ISolicitaExistenciaRadicadoRutaWorkflowService, SolicitaExistenciaRadicadoRutaWorkflowService>();
builder.Services.AddScoped<ISolicitaEstructuraRutaWorkflowService, SolicitaEstructuraRutaWorkflowService>();
builder.Services.AddScoped<IServiceSolicitaEstructuraRespuesta, ServiceSolicitaEstructuraRespuesta>();
builder.Services.AddScoped<IServiceSolicitaListaTiposRespuesta, ServiceSolicitaListaTiposRespuesta>();
builder.Services.AddScoped<IServiceSolicitaDocumentosAdjuntosRespuestaRadicado, ServiceSolicitaDocumentosAdjuntosRespuestaRadicado>();
builder.Services.AddScoped<IServiceSolicitaCorreoElectronicoRemitente, ServiceSolicitaCorreoElectronicoRemitente>();
builder.Services.AddScoped<IServiceSolicitaUsuarioPrincipalRespuesta, ServiceSolicitaUsuarioPrincipalRespuesta>();
builder.Services.AddScoped<IServiceSolicitaListaFirmasPermitidasSolicitudAprobacion, ServiceSolicitaListaFirmasPermitidasSolicitudAprobacion>();
builder.Services.AddScoped<ISolicitaListaFirmasPermitidasSolicitudAprobacionRepository, SolicitaListaFirmasPermitidasSolicitudAprobacionRepository>();
builder.Services.AddScoped<IServiceSolicitaListaFirmasAutorizadasDocumento, ServiceSolicitaListaFirmasAutorizadasDocumento>();
builder.Services.AddScoped<IServiceSolicitaFirmasDocumentoRespuestaOrquestado, ServiceSolicitaFirmasDocumentoRespuestaOrquestado>();
builder.Services.AddScoped<ISolicitaListaFirmasAutorizadasDocumentoRepository, SolicitaListaFirmasAutorizadasDocumentoRepository>();
builder.Services.AddScoped<IServiceSolicitaEstructuraConfiguracionUpload, ServiceSolicitaEstructuraConfiguracionUpload>();
builder.Services.AddScoped<IServiceGuardaEditorDocument, ServiceGuardaEditorDocument>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.IAlmacenarDocumentoUseCase, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.AlmacenarDocumentoUseCase>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.IDocumentStorageOrchestrator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.DocumentStorageOrchestrator>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Transaction.IStorageTransactionCoordinator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Transaction.StorageTransactionCoordinator>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.IStorageValidationPipeline, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.StorageValidationPipeline>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.IStorageValidator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.RequestStructureValidator>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.IStorageValidator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.DocumentoValidator>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.IStorageValidator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.CamposValidator>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.IStorageValidator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.TipoAlmacenamientoValidator>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.IStorageValidator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.ReglasBasicasValidator>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.IStorageValidator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.PreindexValidator>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.IStorageValidator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.GabineteRequiredFieldsValidator>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.IStorageValidator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.StorageOptionsValidator>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.IStorageValidator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.TrdRulesValidator>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.IStorageValidator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Validation.ExpedienteUnidadRulesValidator>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Preindex.IStoragePreindexResolver, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Preindex.StoragePreindexResolver>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Preindex.IStoragePreindexReader, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Preindex.StoragePreindexReader>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Preindex.IStoragePreindexIntegrator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Preindex.StoragePreindexIntegrator>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Metadata.IStorageGabineteMetadataProvider, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Metadata.StorageGabineteMetadataProvider>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Metadata.IStorageDocumentMetadataAnalyzer, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Metadata.StorageDocumentMetadataAnalyzer>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Metadata.IStoragePageCountReader, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Metadata.StoragePageCountReader>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Metadata.IStorageSizeFormatter, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Metadata.StorageSizeFormatter>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Options.IStorageOptionsResolver, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Options.StorageOptionsResolver>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Identity.IStorageIdentityAllocator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Identity.StorageIdentityAllocator>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Identity.IStorageIdentityPolicy, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Identity.StorageIdentityPolicy>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Identity.IStorageDiskQuotaPolicy, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Identity.StorageDiskQuotaPolicy>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Expediente.IIndiceElectronicoCalculator, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Expediente.IndiceElectronicoCalculator>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Expediente.IIndiceElectronicoBuilder, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Expediente.IndiceElectronicoBuilder>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Expediente.IExpedienteUnidadLegacyBuilder, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Expediente.ExpedienteUnidadLegacyBuilder>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Expediente.IExpedienteUnidadLegacyService, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Expediente.ExpedienteUnidadLegacyService>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.ExpedienteXml.IExpedienteIndiceXmlBuilder, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.ExpedienteXml.ExpedienteIndiceXmlBuilder>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.ExpedienteXml.IExpedienteIndiceXmlWriter, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.ExpedienteXml.ExpedienteIndiceXmlWriter>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.ExpedienteXml.IExpedienteIndiceXmlService, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.ExpedienteXml.ExpedienteIndiceXmlService>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Inventario.IInventarioDocumentalBuilder, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Inventario.InventarioDocumentalBuilder>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Builders.IStoragePlanBuilder, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Builders.StoragePlanBuilder>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Builders.IStorageXmlBuilder, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Builders.StorageXmlBuilder>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Physical.IStoragePhysicalPathService, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Physical.StoragePhysicalPathService>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Physical.IStorageFolderLegacyPolicy, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Physical.StorageFolderLegacyPolicy>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Physical.IStoragePathResolver, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Physical.StoragePathResolver>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Physical.IStorageFileWriter, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Physical.StorageFileWriter>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Physical.IStorageXmlWriter, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Physical.StorageXmlWriter>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Physical.IStorageCompensationManager, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Physical.StorageCompensationManager>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Compensation.IStorageDbCompensationService, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Compensation.StorageDbCompensationService>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Physical.IStoragePhysicalPhaseExecutor, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Physical.StoragePhysicalPhaseExecutor>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Workflow.IWorkflowStorageLogBuilder, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Workflow.WorkflowStorageLogBuilder>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Workflow.IWorkflowStorageLogService, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Workflow.WorkflowStorageLogService>();
builder.Services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Gabinete.IGabineteStorageRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Gabinete.GabineteStorageRepository>();
builder.Services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Inventario.IInventarioDocumentalRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Inventario.InventarioDocumentalRepository>();
builder.Services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Expediente.IExpedienteRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Expediente.ExpedienteRepository>();
builder.Services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Expediente.IExpedienteLegacyRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Expediente.ExpedienteLegacyRepository>();
builder.Services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Expediente.IUnidadConservacionLegacyRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Expediente.UnidadConservacionLegacyRepository>();
builder.Services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Expediente.IClaseDocumentoLegacyRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Expediente.ClaseDocumentoLegacyRepository>();
builder.Services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.ExpedienteXml.IExpedienteIndiceXmlRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.ExpedienteXml.ExpedienteIndiceXmlRepository>();
builder.Services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.UnidadConservacion.IUnidadConservacionRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.UnidadConservacion.UnidadConservacionRepository>();
builder.Services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.IndiceElectronico.IIndiceElectronicoRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.IndiceElectronico.IndiceElectronicoRepository>();
builder.Services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Workflow.IWorkflowStorageLogRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Workflow.WorkflowStorageLogRepository>();
builder.Services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Compensation.IStorageDbCompensationRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Compensation.StorageDbCompensationRepository>();
builder.Services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.SystemStorage.ISystemStorageRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.SystemStorage.SystemStorageRepository>();
builder.Services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.SystemOptions.IStorageSystemOptionsRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.SystemOptions.StorageSystemOptionsRepository>();
builder.Services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.StorageRoute.IStorageRouteRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.StorageRoute.StorageRouteRepository>();
builder.Services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Disk.IStorageDiskQuotaRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Disk.StorageDiskQuotaRepository>();
builder.Services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.GabineteMetadata.IStorageGabineteMetadataRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.GabineteMetadata.StorageGabineteMetadataRepository>();
builder.Services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Descriptors.IStorageDescriptorCatalogRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Descriptors.StorageDescriptorCatalogRepository>();
builder.Services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Extension.IStorageExtensionRepository, MiApp.Repository.Repositorio.GestorDocumental.AlmacenamientoDocumental.Extension.StorageExtensionRepository>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Descriptors.IStorageDescriptorResolver, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Descriptors.StorageDescriptorResolver>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Naming.IStorageExtensionResolver, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Naming.StorageExtensionResolver>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Naming.IStorageNamingService, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.Naming.StorageNamingService>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload.IStorageUploadPathResolver, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload.StorageUploadPathResolver>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload.IStorageUploadSessionStore, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload.StorageUploadSessionStore>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload.IStorageUploadPolicy, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload.StorageUploadPolicy>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload.IStorageLargeUploadService, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload.StorageLargeUploadService>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload.IStorageUploadCleanupService, MiApp.Services.Service.GestorDocumental.AlmacenamientoDocumental.TemporaryUpload.StorageUploadCleanupService>();
// ===================================================
  builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.Editor.IServiceFullSaveEditorDocument, MiApp.Services.Service.GestorDocumental.Editor.ServiceFullSaveEditorDocument>();
// Infrastructure (Security + Session)
// ===================================================
  builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.Editor.IServiceFullSaveEditorDocument, MiApp.Services.Service.GestorDocumental.Editor.ServiceFullSaveEditorDocument>();
builder.Services.Configure<PermissionTestSettings>(
builder.Configuration.GetSection("PermissionTest"));
builder.Services.AddScoped<ITokenIssuer, TokenIssuer>();
builder.Services.AddScoped<IIpHelper, IpHelperL>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();  //CAMBIADO:     Retorna el sistema de autenticación actual (JWT o ASP.NET Session) según el contexto
builder.Services.AddScoped<IClaimValidationService, ClaimValidationService>();
builder.Services.AddScoped<DocuArchi.Api.Infrastructure.Features.IFeatureToggleService, DocuArchi.Api.Infrastructure.Features.FeatureToggleService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<SesionActual>();

builder.Services.AddScoped<ISesionActual>(sp => sp.GetRequiredService<SesionActual>());
builder.Services.AddScoped<ISesionGeneral>(sp => sp.GetRequiredService<SesionActual>());
builder.Services.AddScoped<ISesionDocuArchi>(sp => sp.GetRequiredService<SesionActual>());
builder.Services.AddScoped<ISesionGestionDocumental>(sp => sp.GetRequiredService<SesionActual>());
builder.Services.AddScoped<ISesionRadicacion>(sp => sp.GetRequiredService<SesionActual>());
builder.Services.AddScoped<ISesionWorkflow>(sp => sp.GetRequiredService<SesionActual>());

// ===================================================
  builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.Editor.IServiceFullSaveEditorDocument, MiApp.Services.Service.GestorDocumental.Editor.ServiceFullSaveEditorDocument>();
// ASP.NET Session
// ===================================================
  builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.Editor.IServiceFullSaveEditorDocument, MiApp.Services.Service.GestorDocumental.Editor.ServiceFullSaveEditorDocument>();
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
  builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.Editor.IServiceFullSaveEditorDocument, MiApp.Services.Service.GestorDocumental.Editor.ServiceFullSaveEditorDocument>();
// ASP.NET JWT Authentication
// ===================================================
  builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.Editor.IServiceFullSaveEditorDocument, MiApp.Services.Service.GestorDocumental.Editor.ServiceFullSaveEditorDocument>();
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
builder.Services.AddScoped<IGuardaEditorDocumentRepository, GuardaEditorDocumentRepository>();
builder.Services.AddScoped<ISolicitaEditorContextDefinitionRepository, SolicitaEditorContextDefinitionRepository>();
builder.Services.AddScoped<IGuardaEditorDocumentContextRepository, GuardaEditorDocumentContextRepository>();
builder.Services.AddScoped<IServiceGuardaEditorDocumentContext, ServiceGuardaEditorDocumentContext>();
builder.Services.AddScoped<ISincronizaEditorDocumentImagesRepository, SincronizaEditorDocumentImagesRepository>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.Editor.IServiceFullSaveEditorDocument, MiApp.Services.Service.GestorDocumental.Editor.ServiceFullSaveEditorDocument>();
builder.Services.AddScoped<MiApp.Repository.Repositorio.GestorDocumental.Editor.ISolicitaEditorDocumentByIdRepository, MiApp.Repository.Repositorio.GestorDocumental.Editor.SolicitaEditorDocumentByIdRepository>();
builder.Services.AddScoped<MiApp.Services.Service.GestorDocumental.Editor.IServiceSolicitaEditorDocumentById, MiApp.Services.Service.GestorDocumental.Editor.ServiceSolicitaEditorDocumentById>();
builder.Services.AddScoped<ITemplateDefinitionsRepository, TemplateDefinitionsRepository>();
builder.Services.AddScoped<ITemplateTokensRepository, TemplateTokensRepository>();
builder.Services.AddScoped<ITemplateContextRulesRepository, TemplateContextRulesRepository>();
builder.Services.AddScoped<IServiceTemplateDefinitions, ServiceTemplateDefinitions>();
builder.Services.AddScoped<IServiceInitialContentEditor, ServiceInitialContentEditor>();
builder.Services.AddScoped<ISolicitaEditorDocumentByContextRepository, SolicitaEditorDocumentByContextRepository>();
builder.Services.AddScoped<IServiceSolicitaEditorDocumentByContext, ServiceSolicitaEditorDocumentByContext>();
builder.Services.AddScoped<IServiceResolveEditorDocument, ServiceResolveEditorDocument>();
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




