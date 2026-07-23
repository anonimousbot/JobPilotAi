using System.Security.Cryptography;
using System.Text;
using JobPilotAi_Backend.Modules.Identity;

namespace JobPilotAi_Backend.Features.Identity.Register;

public static class RegisterUserMappings
{
    public static RegisterUserResponse ToRegisterUserResponse(this User user) =>
        new(
            user.Id,
            user.Email,
            user.Role.ToString(),
            user.EmailConfirmed,
            user.CreatedAt);

    public static string NormalizeEmail(this string email) =>
        email.Trim().ToLowerInvariant();

    public static string ToSha256Hash(this string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));

        return Convert.ToHexString(bytes);
    }
}
