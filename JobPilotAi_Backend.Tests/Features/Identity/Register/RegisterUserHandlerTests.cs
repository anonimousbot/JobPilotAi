using FluentAssertions;
using JobPilotAi_Backend.Features.Identity.Register;
using JobPilotAi_Backend.Modules.Identity;
using JobPilotAi_Backend.Tests.TestSupport;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;

namespace JobPilotAi_Backend.Tests.Features.Identity.Register;

public sealed class RegisterUserHandlerTests
{
    private static readonly DateTimeOffset RegisteredAt = new(2026, 6, 8, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task HandleAsync_WhenEmailIsAvailable_ShouldCreateFreeUserWithHashedPassword()
    {
        await using var database = new DatabaseFixture();
        await using var dbContext = await database.CreateDbContextAsync();
        var passwordHasher = new PasswordHasher<User>();
        var handler = new RegisterUserHandler(
            dbContext,
            passwordHasher,
            new FixedTimeProvider(RegisteredAt),
            NullLogger<RegisterUserHandler>.Instance);
        var request = new RegisterUserRequest(" Candidate@Example.COM ", "SecurePass123");

        var result = await handler.HandleAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Email.Should().Be("candidate@example.com");
        result.Value.Role.Should().Be(UserRole.FreeUser.ToString());
        result.Value.EmailConfirmed.Should().BeFalse();
        result.Value.CreatedAt.Should().Be(RegisteredAt);

        var user = dbContext.Users.Single();
        user.Email.Should().Be("candidate@example.com");
        user.PasswordHash.Should().NotBe(request.Password);
        passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password)
            .Should()
            .Be(PasswordVerificationResult.Success);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailAlreadyExists_ShouldReturnBusinessError()
    {
        await using var database = new DatabaseFixture();
        await using var dbContext = await database.CreateDbContextAsync();
        var passwordHasher = new PasswordHasher<User>();
        var handler = new RegisterUserHandler(
            dbContext,
            passwordHasher,
            new FixedTimeProvider(RegisteredAt),
            NullLogger<RegisterUserHandler>.Instance);
        var request = new RegisterUserRequest("candidate@example.com", "SecurePass123");
        await handler.HandleAsync(request, CancellationToken.None);

        var result = await handler.HandleAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("auth.email_already_registered");
        dbContext.Users.Should().ContainSingle();
    }
}
