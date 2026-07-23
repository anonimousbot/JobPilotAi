using FluentValidation.Results;

namespace JobPilotAi_Backend.Core.Validation;

public static class ValidationResultExtensions
{
    public static IReadOnlyDictionary<string, string[]> ToErrorDictionary(this ValidationResult result) =>
        result.Errors
            .GroupBy(error => error.PropertyName, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).Distinct().ToArray(),
                StringComparer.Ordinal);
}
