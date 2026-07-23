using JobPilotAi_Backend.Modules.Identity;

namespace JobPilotAi_Backend.Core.Security;

public sealed record AuthContext(Guid UserId, string Email, UserRole Role);
