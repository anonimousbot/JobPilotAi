using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using JobPilotAi_Backend.Modules.Identity;
using Microsoft.Extensions.Options;

namespace JobPilotAi_Backend.Core.Security;

public sealed class AccessTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider)
{
    private readonly JwtOptions _options = options.Value;

    public string CreateAccessToken(User user)
    {
        var now = timeProvider.GetUtcNow();
        var header = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new
        {
            alg = "HS256",
            typ = "JWT"
        }));

        var payload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new
        {
            iss = _options.Issuer,
            sub = user.Id.ToString(),
            email = user.Email,
            role = user.Role.ToString(),
            iat = now.ToUnixTimeSeconds(),
            exp = now.AddMinutes(_options.AccessTokenMinutes).ToUnixTimeSeconds()
        }));

        var unsignedToken = $"{header}.{payload}";
        var signature = Sign(unsignedToken);

        return $"{unsignedToken}.{signature}";
    }

    public AuthContext? ValidateAccessToken(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            return null;
        }

        var unsignedToken = $"{parts[0]}.{parts[1]}";
        var expectedSignature = Sign(unsignedToken);
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.ASCII.GetBytes(expectedSignature),
                Encoding.ASCII.GetBytes(parts[2])))
        {
            return null;
        }

        using var payload = JsonDocument.Parse(Base64UrlDecode(parts[1]));
        var root = payload.RootElement;

        if (!root.TryGetProperty("iss", out var issuer)
            || !string.Equals(issuer.GetString(), _options.Issuer, StringComparison.Ordinal))
        {
            return null;
        }

        if (!root.TryGetProperty("exp", out var expiresAt)
            || expiresAt.GetInt64() <= timeProvider.GetUtcNow().ToUnixTimeSeconds())
        {
            return null;
        }

        if (!root.TryGetProperty("sub", out var subject)
            || !Guid.TryParse(subject.GetString(), out var userId)
            || !root.TryGetProperty("email", out var email)
            || !root.TryGetProperty("role", out var role)
            || !Enum.TryParse<UserRole>(role.GetString(), out var userRole))
        {
            return null;
        }

        return new AuthContext(userId, email.GetString() ?? string.Empty, userRole);
    }

    public static string CreateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public string HashRefreshToken(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private string Sign(string unsignedToken)
    {
        var key = Encoding.UTF8.GetBytes(_options.Key);
        using var hmac = new HMACSHA256(key);
        return Base64UrlEncode(hmac.ComputeHash(Encoding.ASCII.GetBytes(unsignedToken)));
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static byte[] Base64UrlDecode(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        padded = padded.PadRight(padded.Length + ((4 - padded.Length % 4) % 4), '=');
        return Convert.FromBase64String(padded);
    }
}
