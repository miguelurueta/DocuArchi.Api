using System.Reflection;
using DocuArchi.Api.Controllers.GestorDocumental.Expedientes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using MiApp.DTOs.DTOs.GestorDocumental.Expedientes;
using MiApp.DTOs.DTOs.Utilidades;
using MiApp.Repository.Repositorio.GestorDocumental.Expedientes;
using MiApp.Services.Service.GestorDocumental.Expedientes;
using Moq;
using Swashbuckle.AspNetCore.Swagger;
using Xunit;

namespace DocuArchi.Api.Tests;

public sealed class GestionExpedientesApiContractTests
{
    [Theory]
    [InlineData("EXPEDIENTE_EDIT_REQUEST_INVALID", 400)]
    [InlineData("EXPEDIENTE_EDIT_FORBIDDEN", 403)]
    [InlineData("EXPEDIENTE_EDIT_CONCURRENCY", 409)]
    [InlineData("EXPEDIENTE_EDIT_ORGANIZATION_INVALID", 422)]
    [InlineData("EXPEDIENTE_EDIT_FAILED", 500)]
    public async Task GetEditModel_MapsPublishedFailureMatrix(string code, int expectedStatus)
    {
        var editModel = new Mock<IExpedienteEditModelService>();
        editModel.Setup(x => x.GetAsync(330, It.IsAny<string>(), "req-423", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Failure<ExpedienteEditModelDto>(code));
        var controller = CreateController(editModel: editModel.Object);

        var result = await controller.GetEditModel(330, CancellationToken.None);

        var response = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(expectedStatus, response.StatusCode);
        var body = Assert.IsType<AppResponses<ExpedienteEditModelDto>>(response.Value);
        Assert.Equal("req-423", body.meta?.RequestId);
        Assert.DoesNotContain("Exception", System.Text.Json.JsonSerializer.Serialize(body),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetEditModel_Returns200WithInstitutionalWrapper()
    {
        var editModel = new Mock<IExpedienteEditModelService>();
        editModel.Setup(x => x.GetAsync(330, It.IsAny<string>(), "req-423", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AppResponses<ExpedienteEditModelDto>
            {
                success = true,
                message = "OK",
                data = new ExpedienteEditModelDto(),
                meta = new AppMeta { RequestId = "req-423", Status = "success" }
            });
        var controller = CreateController(editModel: editModel.Object);

        var result = await controller.GetEditModel(330, CancellationToken.None);

        var response = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        Assert.True(Assert.IsType<AppResponses<ExpedienteEditModelDto>>(response.Value).success);
    }

    [Theory]
    [InlineData("EXPEDIENTE_EDIT_ORGANIZATION_INVALID")]
    [InlineData("EXPEDIENTE_EDIT_RESPONSIBLE_INVALID")]
    public async Task GetEditModel_MapsFunctionalValidationTo422(string code)
    {
        var editModel = new Mock<IExpedienteEditModelService>();
        editModel.Setup(x => x.GetAsync(330, It.IsAny<string>(), "req-423", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Failure<ExpedienteEditModelDto>(code));
        var controller = CreateController(editModel: editModel.Object);

        var result = await controller.GetEditModel(330, CancellationToken.None);

        var response = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, response.StatusCode);
        Assert.Equal(code, Assert.IsType<AppResponses<ExpedienteEditModelDto>>(response.Value).message);
    }

    [Fact]
    public async Task Update_MapsInvalidResponsibleTo422WithoutChangingWrapper()
    {
        var update = new Mock<IEditarExpedienteUseCase>();
        update.Setup(x => x.ExecuteAsync(330, It.IsAny<EditarExpedienteRequestDto>(), It.IsAny<string>(),
                "req-423", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Failure<EditarExpedienteResponseDto>("EXPEDIENTE_EDIT_RESPONSIBLE_INVALID"));
        var controller = CreateController(update: update.Object);

        var result = await controller.Update(330, new EditarExpedienteRequestDto(), CancellationToken.None);

        var response = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, response.StatusCode);
        var body = Assert.IsType<AppResponses<EditarExpedienteResponseDto>>(response.Value);
        Assert.Equal("req-423", body.meta?.RequestId);
    }

    [Fact]
    public void GeneratedOpenApi_DeclaresProtectedEditModelResponsesAndRequiredOpaqueRowVersion()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(x => x.ApplicationName)
            .Returns(typeof(GestionExpedientesCreateModelController).Assembly.GetName().Name!);
        environment.SetupGet(x => x.EnvironmentName).Returns("Development");
        services.AddSingleton(environment.Object);
        services.AddSingleton<IHostEnvironment>(environment.Object);
        services.AddControllers()
            .ConfigureApplicationPartManager(parts =>
            {
                parts.ApplicationParts.Clear();
                parts.ApplicationParts.Add(new AssemblyPart(typeof(GestionExpedientesCreateModelController).Assembly));
                parts.FeatureProviders.Clear();
                parts.FeatureProviders.Add(new SingleControllerFeatureProvider());
            });
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
            options.SwaggerDoc("gestor-documental", new OpenApiInfo { Title = "GestorDocumental", Version = "v1" }));

        using var provider = services.BuildServiceProvider();
        var document = provider.GetRequiredService<ISwaggerProvider>().GetSwagger("gestor-documental");

        var editModel = document.Paths!["/api/gestion-expedientes/{idExpediente}/edit-model"].Operations![HttpMethod.Get];
        Assert.Equal(new[] { "200", "400", "403", "404", "409", "422", "500" },
            editModel.Responses!.Keys.OrderBy(x => x, StringComparer.Ordinal));

        var update = document.Paths["/api/gestion-expedientes/{idExpediente}"].Operations![HttpMethod.Put];
        Assert.Contains("409", update.Responses!.Keys);
        var resolved = document.Components!.Schemas!["EditarExpedienteRequestDto"];
        Assert.Contains("rowVersion", resolved.Required!);
        Assert.Equal(128, resolved.Properties!["rowVersion"].MaxLength);
    }

    [Fact]
    public void ModularRegistration_ContainsCompleteEditGraph()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddGestionExpedientes(new ConfigurationBuilder().Build());

        AssertRegistration<IExpedienteEditModelService, ExpedienteEditModelService>(services);
        AssertRegistration<IEditarExpedienteUseCase, EditarExpedienteUseCase>(services);
        AssertRegistration<IExpedienteWriteRepository, ExpedienteWriteRepository>(services);
        AssertRegistration<IExpedienteUpdateValidator, ExpedienteUpdateValidator>(services);
        AssertRegistration<IExpedientePermissionPolicy, ExpedientePermissionPolicy>(services);
        AssertRegistration<IExpedienteEstadoPolicy, ExpedienteEstadoPolicy>(services);
        AssertRegistration<IExpedienteOwnershipPolicy, ExpedienteOwnershipPolicy>(services);
        AssertRegistration<IExpedienteAutoRegistroPolicy, ExpedienteAutoRegistroPolicy>(services);
        AssertRegistration<IExpedienteRetencionService, ExpedienteRetencionService>(services);
    }

    private static void AssertRegistration<TService, TImplementation>(IServiceCollection services) =>
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(TService)
            && descriptor.ImplementationType == typeof(TImplementation)
            && descriptor.Lifetime == ServiceLifetime.Scoped);

    private static AppResponses<T> Failure<T>(string code) => new()
    {
        success = false,
        message = code,
        meta = new AppMeta { RequestId = "req-423", Status = "error" },
        errors = [new { Code = code, UserMessage = "Solicitud no válida." }]
    };

    private static GestionExpedientesCreateModelController CreateController(
        IExpedienteEditModelService? editModel = null,
        IEditarExpedienteUseCase? update = null)
    {
        var controller = new GestionExpedientesCreateModelController(
            Mock.Of<IExpedienteCreateModelService>(), Mock.Of<IExpedienteCatalogService>(),
            Mock.Of<ICrearExpedienteUseCase>(), editModel ?? Mock.Of<IExpedienteEditModelService>(),
            update ?? Mock.Of<IEditarExpedienteUseCase>());
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { TraceIdentifier = "req-423" }
        };
        return controller;
    }

    private sealed class SingleControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature) =>
            feature.Controllers.Add(typeof(GestionExpedientesCreateModelController).GetTypeInfo());
    }
}
