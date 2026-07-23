using FluentValidation;
using JobPilotAi_Backend.Core.Results;
using JobPilotAi_Backend.Core.Validation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JobPilotAi_Backend.Features.Identity.Register;

public static class RegisterUserEndpoint
{
    public static IServiceCollection AddRegisterUserFeature(this IServiceCollection services)
    {
        services.AddScoped<RegisterUserHandler>();
        services.AddScoped<IPasswordHasher<Modules.Identity.User>, PasswordHasher<Modules.Identity.User>>();
        services.TryAddSingleton(TimeProvider.System);

        return services;
    }

    public static IEndpointRouteBuilder MapRegisterUserFeature(this IEndpointRouteBuilder app)
    {
        app.MapGroup(AuthRouteConsts.AuthGroup)
            .WithTags("Auth")
            .MapPost(AuthRouteConsts.Register, RegisterAsync)
            .AllowAnonymous()
            .WithName("RegisterUser")
            .WithSummary("Registers a new free user account.");

        return app;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterUserRequest request,
        IValidator<RegisterUserRequest> validator,
        RegisterUserHandler handler,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<RegisterUserResponse>
                .Failure(Error.Validation(validationResult.ToErrorDictionary()))
                .ToHttpResult();
        }

        var result = await handler.HandleAsync(request, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Created($"/api/users/{result.Value!.Id}", result.Value)
            : result.ToHttpResult();
    }
}
