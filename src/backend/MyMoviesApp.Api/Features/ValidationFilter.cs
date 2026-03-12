using System.ComponentModel.DataAnnotations;

namespace MyMoviesApp.Api.Features;

/// <summary>
/// Endpoint filter that runs DataAnnotations validation on the first argument of type
/// <typeparamref name="T"/> and returns a 400 ValidationProblem when it fails.
/// </summary>
public class ValidationFilter<T> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var argument = context.Arguments.OfType<T>().FirstOrDefault();

        if (argument is not null)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(argument);

            if (!Validator.TryValidateObject(argument, validationContext, validationResults, validateAllProperties: true))
            {
                var errors = validationResults
                    .GroupBy(r => r.MemberNames.FirstOrDefault() ?? string.Empty)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(r => r.ErrorMessage ?? "Invalid value.").ToArray());

                return Results.ValidationProblem(errors);
            }
        }

        return await next(context);
    }
}

