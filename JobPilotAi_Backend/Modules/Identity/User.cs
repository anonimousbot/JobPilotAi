namespace JobPilotAi_Backend.Modules.Identity;

public sealed class User
{
    public Guid Id { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public UserRole Role { get; private set; } = UserRole.FreeUser;

    public bool EmailConfirmed { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public UserStatus Status { get; private set; } = UserStatus.Active;

    public static User Register(
        string email,
        string passwordHash,
        DateTimeOffset registeredAt)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            Role = UserRole.FreeUser,
            EmailConfirmed = false,
            CreatedAt = registeredAt,
            UpdatedAt = registeredAt,
            Status = UserStatus.Active
        };
    }

    public void ConfirmEmail(DateTimeOffset confirmedAt)
    {
        EmailConfirmed = true;
        UpdatedAt = confirmedAt;
    }

    public void ChangePassword(string passwordHash, DateTimeOffset changedAt)
    {
        PasswordHash = passwordHash;
        UpdatedAt = changedAt;
    }
}
