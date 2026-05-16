using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DocuArchi.Api.Infrastructure.Swagger
{
    public sealed class UploadTemporalChunkOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var method = context.ApiDescription.HttpMethod;
            var relativePath = context.ApiDescription.RelativePath ?? string.Empty;

            var isChunkEndpointPath =
                relativePath.Contains("/upload-temporal/", StringComparison.OrdinalIgnoreCase) &&
                relativePath.Contains("/chunk/{chunkIndex}", StringComparison.OrdinalIgnoreCase);

            var isSupportedControllerPath =
                relativePath.Contains("api/gestor-documental/almacenamiento/", StringComparison.OrdinalIgnoreCase) ||
                relativePath.Contains("api/gestor-documental/documentos/reemplazopdf/", StringComparison.OrdinalIgnoreCase);

            var isChunkUploadEndpoint =
                string.Equals(method, "PUT", StringComparison.OrdinalIgnoreCase) &&
                isChunkEndpointPath &&
                isSupportedControllerPath;

            if (!isChunkUploadEndpoint)
            {
                return;
            }

            if (operation.Parameters == null)
            {
                operation.Parameters = new List<IOpenApiParameter>();
            }

            if (!operation.Parameters.Any(p =>
                string.Equals(p.Name, "X-Total-Chunks", StringComparison.OrdinalIgnoreCase) &&
                p.In == ParameterLocation.Header))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "X-Total-Chunks",
                    In = ParameterLocation.Header,
                    Required = true,
                    Description = "Cantidad total de chunks del archivo.",
                    Schema = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32", Minimum = "1" }
                });
            }

            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/octet-stream"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = JsonSchemaType.String,
                            Format = "binary"
                        }
                    }
                }
            };
        }
    }
}
