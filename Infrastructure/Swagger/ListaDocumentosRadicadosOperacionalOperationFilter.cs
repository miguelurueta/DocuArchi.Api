using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DocuArchi.Api.Infrastructure.Swagger;

/// <summary>
/// Publica en OpenAPI las combinaciones válidas del listado operacional sin
/// convertir datos aportados por el cliente en autoridad.
/// </summary>
public sealed class ListaDocumentosRadicadosOperacionalOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var relativePath = context.ApiDescription.RelativePath ?? string.Empty;
        if (!string.Equals(context.ApiDescription.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase) ||
            !relativePath.Equals(
                "api/GestorDocumental/Documentos/ListaDocumentosRadicadosOperacional/query",
                StringComparison.OrdinalIgnoreCase))
            return;

        operation.Summary = "Lista documentos de Radicación o Workflow con contexto autorizado";
        operation.Description = """
            El backend obtiene identidad, aliases, actividad y permisos desde el JWT; esos valores no se reciben en el body.

            Modos válidos:
            - RADICACION: ResolutionSource=RADICACION, SourceModule=RADICACION, sin WorkflowAccessMode.
            - Gestión de Respuesta: ResolutionSource=WORKFLOW, SourceModule=WORKFLOW, WorkflowAccessMode=GESTION_RESPUESTA. Este es el default cuando el modo se omite.
            - Bandeja Workflow: ResolutionSource=WORKFLOW, SourceModule=WORKFLOW, WorkflowAccessMode=WORKFLOW_INBOX.

            Ejemplo RADICACION:
            { "radicado": "2600466700064", "resolutionSource": "RADICACION", "tipo": "entrante", "sourceModule": "RADICACION", "documentRelationScope": "documentsOnly", "page": 1, "pageSize": 25, "structuredFilters": [], "includeConfig": true, "viewMode": "flatDocuments" }

            Ejemplo GESTION_RESPUESTA:
            { "radicado": "2500466700035", "resolutionSource": "WORKFLOW", "idRutaWorkflow": 9, "idTareaWorkflow": 933, "workflowAccessMode": "GESTION_RESPUESTA", "sourceModule": "WORKFLOW", "documentRelationScope": "documentsOnly", "page": 1, "pageSize": 25, "structuredFilters": [], "includeConfig": true, "viewMode": "flatDocuments" }

            Ejemplo WORKFLOW_INBOX:
            { "radicado": "2500466700009", "resolutionSource": "WORKFLOW", "idRutaWorkflow": 9, "idTareaWorkflow": 910, "workflowAccessMode": "WORKFLOW_INBOX", "sourceModule": "WORKFLOW", "documentRelationScope": "documentsOnly", "page": 1, "pageSize": 25, "structuredFilters": [], "includeConfig": true, "viewMode": "flatDocuments" }

            Respuestas: 200 éxito o vacío; 400 request/modo inválido; 401 sin autenticación; 403 sin autoridad; 404 contexto inexistente; 409 conflicto con contexto canónico; 422 alcance no soportado; 500 fallo inesperado.
            """;
    }
}
