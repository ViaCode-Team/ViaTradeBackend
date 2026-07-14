using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Nodes;

namespace ViaTradeBackend.Swagger.Filters
{
    public class ProblemDetailsSchemaFilter : ISchemaFilter
    {
        public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type != typeof(ProblemDetails))
                return;

            if (schema is not OpenApiSchema openApiSchema)
                return;

            openApiSchema.Type = JsonSchemaType.Object;
            openApiSchema.Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["type"] = new OpenApiSchema { Type = JsonSchemaType.String, Format = "uri" },
                ["title"] = new OpenApiSchema { Type = JsonSchemaType.String },
                ["status"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" },
                ["detail"] = new OpenApiSchema { Type = JsonSchemaType.String },
            };

            openApiSchema.Example = new JsonObject
            {
                ["type"] = "https://httpstatuses.com/400",
                ["title"] = "Bad Request",
                ["status"] = 400,
                ["detail"] = "Invalid input parameter",
            };
        }
    }
}