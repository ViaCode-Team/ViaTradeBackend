using Domain.Models.Pagination;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ViaTradeBackend.Swagger.Filters;

public sealed class PaginationRequestOperationFilter : IOperationFilter
{
    private static readonly HashSet<string> PaginationParameterNames =
    [
        nameof(PaginationRequest.Page),
        nameof(PaginationRequest.PageSize)
    ];

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters is null)
            return;

        var hasPagination = context.ApiDescription.ActionDescriptor.Parameters
            .Any(parameter => parameter.ParameterType == typeof(PaginationRequest));

        if (!hasPagination)
            return;


        var parametersToRemove = operation.Parameters
            .Where(parameter =>
                parameter.Name is not null &&
                PaginationParameterNames.Contains(parameter.Name))
            .ToList();

        foreach (var parameter in parametersToRemove)
        {
            operation.Parameters.Remove(parameter);
        }

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "pagination",
            In = ParameterLocation.Query,
            Schema = context.SchemaGenerator.GenerateSchema(
                typeof(PaginationRequest),
                context.SchemaRepository),
            Style = ParameterStyle.Form,
            Explode = true
        });
    }
}