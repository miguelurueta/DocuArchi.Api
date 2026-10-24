using DocuArchi.Api.Controllers.GestionCorrespondencia.GestionRespuesta.Core.Preparacion;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DocuArchi.Api.Infrastructure.Swagger;

public sealed class PreparacionRespuestaOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(PreparacionRespuestaController)) return;
        operation.Summary = "Preparar formulario de respuesta sin escrituras de negocio";
        operation.Description = """
            Requiere JWT, permiso ra.gestion.respuesta y pertenencia a empresa, área y respuesta/tarea.
            Actor y alias se obtienen de claims. idRespuestaRadicado e idTareaWf son referencias a comprobar.
            Ejemplo sintético: GET /api/gestion-correspondencia/respuestas/5/preparacion?idTareaWf=7.
            200 entrega datos y posibles bloqueos funcionales; puedeConfirmar siempre false en este corte.
            400 contrato/claims inválidos; 401 no autenticado; 403 sin acceso o respuesta no accesible;
            409 relación tarea/respuesta inconsistente; 500 lectura fallida con diagnóstico seguro y requestId.
            El correo principal es de solo lectura. No incluye adjuntos/cantidad ni inicia PDF, firma,
            expediente, envío, operación durable o modificación Workflow. No requiere clave de idempotencia.
            referenciaPreparacion es una huella del servidor para la futura relectura; no autoriza confirmar.
            La evidencia de aprobación sin vínculo documental verificable produce bloqueo, nunca aprobación implícita.
            """;
    }
}

public sealed class PreparacionRespuestaSecurityFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        if (!document.Paths.TryGetValue("/api/gestion-correspondencia/respuestas/{idRespuestaRadicado}/preparacion", out var path)) return;
        foreach (var operation in path.Operations!.Values)
            operation.Security = [new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("Bearer", document)] = [] }];
    }
}
