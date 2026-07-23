using FluentValidation;

namespace JobPilotAi_Backend.Core.Validation;

public static class ValidationServiceCollectionExtensions
{
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<Program>(includeInternalTypes: true);

        return services;
    }
}
