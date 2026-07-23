namespace JobPilotAi_Backend.Modules.Profiles;

public sealed class Profile
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public string? PhoneNumber { get; private set; }

    public string? Location { get; private set; }

    public string? LinkedInUrl { get; private set; }

    public string? PortfolioUrl { get; private set; }

    public string? CareerGoal { get; private set; }

    public string? TargetRole { get; private set; }

    public string? CareerLevel { get; private set; }

    public string? CurrentJobTitle { get; private set; }

    public int? YearsOfExperience { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static Profile Create(Guid userId, DateTimeOffset createdAt) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };

    public void Update(
        string firstName,
        string lastName,
        string? phoneNumber,
        string? location,
        string? linkedInUrl,
        string? portfolioUrl,
        string? careerGoal,
        string? targetRole,
        string? careerLevel,
        DateTimeOffset updatedAt)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();
        Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim();
        LinkedInUrl = string.IsNullOrWhiteSpace(linkedInUrl) ? null : linkedInUrl.Trim();
        PortfolioUrl = string.IsNullOrWhiteSpace(portfolioUrl) ? null : portfolioUrl.Trim();
        CareerGoal = string.IsNullOrWhiteSpace(careerGoal) ? null : careerGoal.Trim();
        TargetRole = string.IsNullOrWhiteSpace(targetRole) ? null : targetRole.Trim();
        CareerLevel = string.IsNullOrWhiteSpace(careerLevel) ? null : careerLevel.Trim();
        UpdatedAt = updatedAt;
    }

    public void UpdateOnboarding(
        string careerGoal,
        string targetRole,
        string careerLevel,
        string? currentJobTitle,
        int? yearsOfExperience,
        DateTimeOffset updatedAt)
    {
        CareerGoal = careerGoal.Trim();
        TargetRole = targetRole.Trim();
        CareerLevel = careerLevel.Trim();
        CurrentJobTitle = string.IsNullOrWhiteSpace(currentJobTitle) ? null : currentJobTitle.Trim();
        YearsOfExperience = yearsOfExperience;
        UpdatedAt = updatedAt;
    }
}
