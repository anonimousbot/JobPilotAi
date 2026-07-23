using System.Text.Json;
using FluentValidation;
using JobPilotAi_Backend.Core.Results;
using JobPilotAi_Backend.Core.Security;
using JobPilotAi_Backend.Core.Validation;
using JobPilotAi_Backend.Infrastructure.Persistence;
using JobPilotAi_Backend.Modules.JobMatches;
using Microsoft.EntityFrameworkCore;

namespace JobPilotAi_Backend.Features.JobMatches;

public static class JobMatchFeature
{
    private static readonly JobMatchSeed[] Catalog =
    [
        new(
            "senior-product-designer-lumina",
            "Senior Product Designer",
            "Lumina Systems",
            "Remote",
            94,
            ["Figma", "AI/ML UX", "Design Systems"],
            ["Product Design", "Design Systems", "Figma", "AI Data Visualization", "Leadership"],
            "High alignment for product design, design systems, and AI dashboard experience."),
        new(
            "staff-frontend-engineer-dataflow",
            "Staff Frontend Engineer",
            "DataFlow Inc.",
            "New York, NY",
            88,
            ["React", "TypeScript", "D3.js"],
            ["React", "TypeScript", "Data Visualization", "Performance", "Mentorship"],
            "Strong technical fit for frontend architecture and analytical product surfaces."),
        new(
            "lead-ux-researcher-nexus",
            "Lead UX Researcher",
            "Nexus Creative",
            "Austin, TX",
            72,
            ["User Testing", "Cognitive Psych"],
            ["User Research", "User Testing", "Journey Mapping", "Stakeholder Management"],
            "Moderate match with clear room to strengthen research leadership keywords."),
        new(
            "director-engineering-pioneer",
            "Director of Engineering",
            "Pioneer Robotics",
            "San Francisco, CA",
            81,
            ["LLM Scaling", "Leadership", "Robotics"],
            ["Engineering Leadership", "LLM Scaling", "Hiring", "Roadmapping", "Execution"],
            "Leadership-forward role with strong upside if AI scaling language is explicit.")
    ];

    public static IEndpointRouteBuilder MapJobMatchFeature(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/jobmatches").WithTags("Job Matches");

        group.MapGet("", ListSavedAsync)
            .WithName("ListSavedJobMatches")
            .WithSummary("Lists saved job matches for the authenticated user.");

        group.MapPost("/search", SearchAsync)
            .WithName("SearchJobMatches")
            .WithSummary("Searches job matches for the authenticated user.");

        group.MapPost("/{id}/compare", CompareAsync)
            .WithName("CompareJobMatch")
            .WithSummary("Compares one job match against the authenticated user's profile.");

        group.MapPost("/{id}/save", SaveAsync)
            .WithName("SaveJobMatch")
            .WithSummary("Saves one job match for the authenticated user.");

        group.MapDelete("/{id}/save", UnsaveAsync)
            .WithName("UnsaveJobMatch")
            .WithSummary("Removes one saved job match for the authenticated user.");

        return app;
    }

    private static async Task<IResult> ListSavedAsync(
        CurrentUserAccessor currentUser,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var auth = currentUser.GetRequiredUser();
        if (!auth.IsSuccess)
        {
            return auth.ToHttpResult();
        }

        var savedMatches = await dbContext.SavedJobMatches
            .Where(match => match.UserId == auth.Value!.UserId)
            .OrderByDescending(match => match.SavedAt)
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(savedMatches.Select(ToResponse).ToArray());
    }

    private static async Task<IResult> SearchAsync(
        JobMatchSearchRequest request,
        IValidator<JobMatchSearchRequest> validator,
        CurrentUserAccessor currentUser,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<JobMatchResponse[]>.Failure(Error.Validation(validationResult.ToErrorDictionary())).ToHttpResult();
        }

        var auth = currentUser.GetRequiredUser();
        if (!auth.IsSuccess)
        {
            return auth.ToHttpResult();
        }

        if (!string.IsNullOrWhiteSpace(request.ResumeId))
        {
            var resumeId = Guid.Parse(request.ResumeId);
            var resumeExists = await dbContext.Resumes
                .AnyAsync(resume => resume.Id == resumeId && resume.UserId == auth.Value!.UserId, cancellationToken);
            if (!resumeExists)
            {
                return Result<JobMatchResponse[]>.Failure(Error.NotFound("resume.not_found", "The resume was not found.")).ToHttpResult();
            }
        }

        var savedIds = await dbContext.SavedJobMatches
            .Where(match => match.UserId == auth.Value!.UserId)
            .Select(match => match.JobMatchId)
            .ToListAsync(cancellationToken);
        var saved = savedIds.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var query = request.Query?.Trim();
        var location = request.Location?.Trim();
        var matches = Catalog
            .Where(match => MatchesSearch(match, query, location))
            .OrderByDescending(match => match.MatchScore)
            .Select(match => ToResponse(match, saved.Contains(match.Id)))
            .ToArray();

        return TypedResults.Ok(matches);
    }

    private static async Task<IResult> CompareAsync(
        string id,
        CurrentUserAccessor currentUser,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var auth = currentUser.GetRequiredUser();
        if (!auth.IsSuccess)
        {
            return auth.ToHttpResult();
        }
        var userId = auth.Value!.UserId;

        var match = FindMatch(id);
        if (match is null)
        {
            return Result<JobMatchComparisonResponse>.Failure(Error.NotFound("job_match.not_found", "The job match was not found.")).ToHttpResult();
        }

        var profile = await dbContext.Profiles
            .SingleOrDefaultAsync(item => item.UserId == auth.Value!.UserId, cancellationToken);
        var latestAnalysis = await dbContext.ResumeAnalyses
            .Where(item => item.UserId == auth.Value!.UserId)
            .OrderByDescending(item => item.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var strengths = latestAnalysis is null
            ? []
            : ReadJsonArray(latestAnalysis.StrengthsJson);
        var matchedSkills = match.RequiredSkills
            .Where(skill => strengths.Any(strength => strength.Contains(skill, StringComparison.OrdinalIgnoreCase))
                || (profile?.TargetRole?.Contains(skill, StringComparison.OrdinalIgnoreCase) ?? false))
            .DefaultIfEmpty(match.Tags[0])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var missingSkills = match.RequiredSkills
            .Except(matchedSkills, StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .ToArray();

        var response = new JobMatchComparisonResponse(
            match.Id,
            match.JobTitle,
            match.CompanyName,
            match.MatchScore,
            match.RequiredSkills,
            matchedSkills,
            missingSkills,
            [
                $"Add {missingSkills.FirstOrDefault() ?? match.Tags[0]} language to your resume summary.",
                $"Tailor one cover letter paragraph to {match.CompanyName}'s role requirements.",
                "Run a fresh ATS analysis with this job description before applying."
            ],
            $"Your current profile is a {match.MatchScore}% match for {match.JobTitle}. The fastest lift is making the missing skills explicit.");

        return TypedResults.Ok(response);
    }

    private static async Task<IResult> SaveAsync(
        string id,
        CurrentUserAccessor currentUser,
        ApplicationDbContext dbContext,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var auth = currentUser.GetRequiredUser();
        if (!auth.IsSuccess)
        {
            return auth.ToHttpResult();
        }
        var userId = auth.Value!.UserId;

        var match = FindMatch(id);
        if (match is null)
        {
            return Result<SavedJobMatchResponse>.Failure(Error.NotFound("job_match.not_found", "The job match was not found.")).ToHttpResult();
        }

        var existing = await dbContext.SavedJobMatches.SingleOrDefaultAsync(
            item => item.UserId == userId && item.JobMatchId == match.Id,
            cancellationToken);
        if (existing is not null)
        {
            return TypedResults.Ok(ToResponse(existing));
        }

        var saved = SavedJobMatch.Create(
            userId,
            match.Id,
            match.JobTitle,
            match.CompanyName,
            match.Location,
            match.MatchScore,
            JsonSerializer.Serialize(match.Tags),
            timeProvider.GetUtcNow());

        dbContext.SavedJobMatches.Add(saved);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/api/jobmatches/{saved.JobMatchId}", ToResponse(saved));
    }

    private static async Task<IResult> UnsaveAsync(
        string id,
        CurrentUserAccessor currentUser,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var auth = currentUser.GetRequiredUser();
        if (!auth.IsSuccess)
        {
            return auth.ToHttpResult();
        }

        var saved = await dbContext.SavedJobMatches.SingleOrDefaultAsync(
            item => item.UserId == auth.Value!.UserId && item.JobMatchId == id,
            cancellationToken);
        if (saved is null)
        {
            return TypedResults.NoContent();
        }

        dbContext.SavedJobMatches.Remove(saved);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }

    private static bool MatchesSearch(JobMatchSeed match, string? query, string? location)
    {
        var matchesQuery = string.IsNullOrWhiteSpace(query)
            || match.JobTitle.Contains(query, StringComparison.OrdinalIgnoreCase)
            || match.CompanyName.Contains(query, StringComparison.OrdinalIgnoreCase)
            || match.Tags.Any(tag => tag.Contains(query, StringComparison.OrdinalIgnoreCase));

        var matchesLocation = string.IsNullOrWhiteSpace(location)
            || match.Location.Contains(location, StringComparison.OrdinalIgnoreCase);

        return matchesQuery && matchesLocation;
    }

    private static JobMatchSeed? FindMatch(string id) =>
        Catalog.SingleOrDefault(match => string.Equals(match.Id, id, StringComparison.OrdinalIgnoreCase));

    private static JobMatchResponse ToResponse(JobMatchSeed match, bool saved) =>
        new(match.Id, match.JobTitle, match.CompanyName, match.Location, match.MatchScore, match.Tags, match.Summary, saved);

    private static SavedJobMatchResponse ToResponse(SavedJobMatch match) =>
        new(
            match.Id,
            match.JobMatchId,
            match.JobTitle,
            match.CompanyName,
            match.Location,
            match.MatchScore,
            ReadJsonArray(match.TagsJson),
            match.SavedAt);

    private static IReadOnlyCollection<string> ReadJsonArray(string json) =>
        JsonSerializer.Deserialize<string[]>(json) ?? [];

    private sealed record JobMatchSeed(
        string Id,
        string JobTitle,
        string CompanyName,
        string Location,
        int MatchScore,
        string[] Tags,
        string[] RequiredSkills,
        string Summary);
}
