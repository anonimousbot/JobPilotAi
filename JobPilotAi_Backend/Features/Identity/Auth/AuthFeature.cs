using FluentValidation;
using JobPilotAi_Backend.Core.Results;
using JobPilotAi_Backend.Core.Security;
using JobPilotAi_Backend.Core.Validation;
using JobPilotAi_Backend.Features.Identity.Register;
using JobPilotAi_Backend.Infrastructure.Persistence;
using JobPilotAi_Backend.Modules.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace JobPilotAi_Backend.Features.Identity.Auth;

public static class AuthFeature
{
    public static IEndpointRouteBuilder MapAuthFeature(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(AuthRouteConsts.AuthGroup).WithTags("Auth").RequireCors("Frontend");

        group.MapPost(AuthRouteConsts.Login, LoginAsync)
            .AllowAnonymous()
            .WithName("Login")
            .WithSummary("Authenticates a user and issues access and refresh tokens.");

        group.MapPost(AuthRouteConsts.Refresh, RefreshAsync)
            .AllowAnonymous()
            .WithName("RefreshToken")
            .WithSummary("Rotates a refresh token and issues a new access token.");

        group.MapPost(AuthRouteConsts.Logout, LogoutAsync)
            .WithName("Logout")
            .WithSummary("Revokes the supplied refresh token.");

        group.MapPost(AuthRouteConsts.VerifyEmail, VerifyEmailAsync)
            .AllowAnonymous()
            .WithName("VerifyEmail")
            .WithSummary("Marks a user's email address as verified for MVP environments.");

        group.MapPost(AuthRouteConsts.ForgotPassword, ForgotPasswordAsync)
            .AllowAnonymous()
            .WithName("ForgotPassword")
            .WithSummary("Accepts a password reset request without exposing account existence.");

        group.MapPost(AuthRouteConsts.ResetPassword, ResetPasswordAsync)
            .AllowAnonymous()
            .WithName("ResetPassword")
            .WithSummary("Resets a password in MVP environments.");

        return app;
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        IValidator<LoginRequest> validator,
        ApplicationDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        AccessTokenService tokenService,
        IOptions<JwtOptions> jwtOptions,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<AuthResponse>.Failure(Error.Validation(validationResult.ToErrorDictionary())).ToHttpResult();
        }

        var normalizedEmail = request.Email.NormalizeEmail();
        var user = await dbContext.Users.SingleOrDefaultAsync(item => item.Email == normalizedEmail, cancellationToken);
        if (user is null
            || passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            return Result<AuthResponse>.Failure(Error.Unauthorized("auth.invalid_credentials", "The email or password is incorrect.")).ToHttpResult();
        }

        var response = CreateTokenResponse(user, dbContext, tokenService, jwtOptions.Value, timeProvider.GetUtcNow());
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(response);
    }

    private static async Task<IResult> RefreshAsync(
        RefreshTokenRequest request,
        IValidator<RefreshTokenRequest> validator,
        ApplicationDbContext dbContext,
        AccessTokenService tokenService,
        IOptions<JwtOptions> jwtOptions,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<AuthResponse>.Failure(Error.Validation(validationResult.ToErrorDictionary())).ToHttpResult();
        }

        var now = timeProvider.GetUtcNow();
        var tokenHash = tokenService.HashRefreshToken(request.RefreshToken);
        var refreshToken = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(item => item.TokenHash == tokenHash, cancellationToken);

        if (refreshToken is null || !refreshToken.IsActive(now))
        {
            return Result<AuthResponse>.Failure(Error.Unauthorized("auth.invalid_refresh_token", "The refresh token is invalid or expired.")).ToHttpResult();
        }

        var user = await dbContext.Users.FindAsync([refreshToken.UserId], cancellationToken);
        if (user is null)
        {
            return Result<AuthResponse>.Failure(Error.Unauthorized("auth.user_missing", "The token user no longer exists.")).ToHttpResult();
        }

        refreshToken.Revoke(now);
        var response = CreateTokenResponse(user, dbContext, tokenService, jwtOptions.Value, now);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(response);
    }

    private static async Task<IResult> LogoutAsync(
        RefreshTokenRequest request,
        ApplicationDbContext dbContext,
        AccessTokenService tokenService,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var tokenHash = tokenService.HashRefreshToken(request.RefreshToken);
        var refreshToken = await dbContext.RefreshTokens.SingleOrDefaultAsync(item => item.TokenHash == tokenHash, cancellationToken);
        refreshToken?.Revoke(timeProvider.GetUtcNow());
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<IResult> VerifyEmailAsync(
        VerifyEmailRequest request,
        IValidator<VerifyEmailRequest> validator,
        ApplicationDbContext dbContext,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<object>.Failure(Error.Validation(validationResult.ToErrorDictionary())).ToHttpResult();
        }

        var normalizedEmail = request.Email.NormalizeEmail();
        var user = await dbContext.Users.SingleOrDefaultAsync(item => item.Email == normalizedEmail, cancellationToken);
        if (user is null)
        {
            return Result<object>.Failure(Error.NotFound("auth.user_not_found", "The user was not found.")).ToHttpResult();
        }

        user.ConfirmEmail(timeProvider.GetUtcNow());
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(new { success = true });
    }

    private static IResult ForgotPasswordAsync(ForgotPasswordRequest request) =>
        TypedResults.Ok(new { success = true });

    private static async Task<IResult> ResetPasswordAsync(
        ResetPasswordRequest request,
        IValidator<ResetPasswordRequest> validator,
        ApplicationDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<object>.Failure(Error.Validation(validationResult.ToErrorDictionary())).ToHttpResult();
        }

        var normalizedEmail = request.Email.NormalizeEmail();
        var user = await dbContext.Users.SingleOrDefaultAsync(item => item.Email == normalizedEmail, cancellationToken);
        if (user is not null)
        {
            user.ChangePassword(passwordHasher.HashPassword(user, request.NewPassword), timeProvider.GetUtcNow());
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return TypedResults.Ok(new { success = true });
    }

    private static AuthResponse CreateTokenResponse(
        User user,
        ApplicationDbContext dbContext,
        AccessTokenService tokenService,
        JwtOptions jwtOptions,
        DateTimeOffset now)
    {
        var accessToken = tokenService.CreateAccessToken(user);
        var refreshToken = AccessTokenService.CreateRefreshToken();
        dbContext.RefreshTokens.Add(RefreshToken.Create(user.Id, tokenService.HashRefreshToken(refreshToken), now, now.AddDays(30)));

        return new AuthResponse(
            user.Id,
            user.Email,
            user.Role.ToString(),
            user.EmailConfirmed,
            accessToken,
            refreshToken,
            now.AddMinutes(jwtOptions.AccessTokenMinutes));
    }
}
