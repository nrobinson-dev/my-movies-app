using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MyMoviesApp.Api.OpenApi;

/// <summary>
/// Applies the <c>bearerAuth</c> security requirement to any operation whose endpoint
/// metadata contains an <see cref="IAuthorizeData"/> attribute (i.e. endpoints marked
/// with <c>.RequireAuthorization()</c> or <c>[Authorize]</c>).
/// </summary>
public sealed class BearerSecuritySchemeTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var hasAuthorize = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<IAuthorizeData>()
            .Any();

        if (hasAuthorize)
        {
            operation.Security =
            [
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("bearerAuth")] = []
                }
            ];
        }

        return Task.CompletedTask;
    }
}
