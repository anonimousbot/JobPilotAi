using JobPilotAi_Backend.Core.Results;
using JobPilotAi_Backend.Infrastructure.Persistence;
using JobPilotAi_Backend.Modules.Identity;
using JobPilotAi_Backend.Modules.Profiles;
using JobPilotAi_Backend.Modules.Subscriptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JobPilotAi_Backend.Features.Identity.Register;

public sealed class RegisterUserHandler(
    ApplicationDbContext dbContext,
    IPasswordHasher<User> passwordHasher,
    TimeProvider timeProvider,
    ILogger<RegisterUserHandler> logger)
{
    public async Task<Result<RegisterUserResponse>> HandleAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.NormalizeEmail();

        var emailAlreadyRegistered = await dbContext.Users
            .AnyAsync(user => user.Email == normalizedEmail, cancellationToken);

        if (emailAlreadyRegistered)
        {
            logger.LogInformation(
                "Registration rejected because email is already registered. EmailHash: {EmailHash}",
                normalizedEmail.ToSha256Hash());

            return Result<RegisterUserResponse>.Failure(
                Error.BadRequest("auth.email_already_registered", "An account already exists for this email address."));
        }

        var registeredAt = timeProvider.GetUtcNow();
        var passwordHash = passwordHasher.HashPassword(null!, request.Password);
        var user = User.Register(normalizedEmail, passwordHash, registeredAt);

        dbContext.Users.Add(user);
        dbContext.Profiles.Add(Profile.Create(user.Id, registeredAt));
        dbContext.Subscriptions.Add(Subscription.CreateFree(user.Id, registeredAt));
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "User registered successfully. UserId: {UserId}, Role: {Role}",
            user.Id,
            user.Role);

        return Result<RegisterUserResponse>.Success(user.ToRegisterUserResponse());
    }
}
