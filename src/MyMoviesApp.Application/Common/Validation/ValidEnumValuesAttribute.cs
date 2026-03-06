using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace MyMoviesApp.Application.Common.Validation;

/// <summary>
/// Validates that every value in an enum collection (or a single enum value) is a
/// defined member of its enum type, preventing out-of-range integers from being
/// accepted as valid enum values.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class ValidEnumValuesAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        // Handle collections (HashSet<T>, List<T>, etc.)
        if (value is IEnumerable enumerable and not string)
        {
            var invalidValues = new List<object>();

            foreach (var item in enumerable)
            {
                if (item is not null && item.GetType().IsEnum && !Enum.IsDefined(item.GetType(), item))
                    invalidValues.Add(item);
            }

            if (invalidValues.Count > 0)
            {
                return new ValidationResult(
                    $"The field {validationContext.DisplayName} contains invalid values: {string.Join(", ", invalidValues)}.");
            }

            return ValidationResult.Success;
        }

        // Handle a single enum value
        var type = value.GetType();
        if (type.IsEnum && !Enum.IsDefined(type, value))
        {
            return new ValidationResult(
                $"The value '{value}' is not valid for {validationContext.DisplayName}.");
        }

        return ValidationResult.Success;
    }
}

