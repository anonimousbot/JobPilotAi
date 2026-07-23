using JobPilotAi_Backend.Core.Results;
using JobPilotAi_Backend.Core.Security;
using JobPilotAi_Backend.Infrastructure.Persistence;
using JobPilotAi_Backend.Modules.Identity;
using JobPilotAi_Backend.Modules.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace JobPilotAi_Backend.Features.Admin;

public static class AdminFeature
{
    public static IEndpointRouteBuilder MapAdminFeature(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin").WithTags("Admin");

        group.MapGet("/users", UsersAsync).WithName("AdminUsers");
        group.MapGet("/analytics", AnalyticsAsync).WithName("AdminAnalytics");
        group.MapGet("/auditlogs", AuditLogsAsync).WithName("AdminAuditLogs");

        return app;
    }

    private static async Task<IResult> UsersAsync(
        CurrentUserAccessor currentUser,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var auth = RequireAdmin(currentUser);
        if (!auth.IsSuccess)
        {
            return auth.ToHttpResult();
        }

        var users = await dbContext.Users
            .OrderByDescending(user => user.CreatedAt)
            .Select(user => new AdminUserResponse(
                user.Id,
                user.Email,
                user.Role.ToString(),
                user.EmailConfirmed,
                user.Status.ToString(),
                user.CreatedAt))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(users);
    }

    private static async Task<IResult> AnalyticsAsync(
        CurrentUserAccessor currentUser,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var auth = RequireAdmin(currentUser);
        if (!auth.IsSuccess)
        {
            return auth.ToHttpResult();
        }

        var response = new AdminAnalyticsResponse(
            await dbContext.Users.CountAsync(cancellationToken),
            await dbContext.Resumes.CountAsync(cancellationToken),
            await dbContext.ResumeAnalyses.CountAsync(cancellationToken),
            await dbContext.CoverLetters.CountAsync(cancellationToken),
            await dbContext.Subscriptions.CountAsync(item => item.Status == SubscriptionStatus.Active, cancellationToken),
            await dbContext.UsageRecords.SumAsync(item => item.EstimatedCost ?? 0m, cancellationToken));

        return TypedResults.Ok(response);
    }

    private static async Task<IResult> AuditLogsAsync(
        CurrentUserAccessor currentUser,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var auth = RequireAdmin(currentUser);
        if (!auth.IsSuccess)
        {
            return auth.ToHttpResult();
        }

        var logs = await dbContext.AdminLogs
            .OrderByDescending(log => log.CreatedAt)
            .Take(200)
            .Select(log => new AdminLogResponse(
                log.Id,
                log.AdminId,
                log.Action,
                log.TargetEntity,
                log.TargetId,
                log.MetadataJson,
                log.CreatedAt))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(logs);
    }

    private static Result<AuthContext> RequireAdmin(CurrentUserAccessor currentUser)
    {
        var auth = currentUser.GetRequiredUser();
        if (!auth.IsSuccess)
        {
            return auth;
        }

        return auth.Value!.Role is UserRole.Admin or UserRole.SuperAdmin
            ? auth
            : Result<AuthContext>.Failure(Error.Forbidden("admin.required", "Administrator access is required."));
    }
}
