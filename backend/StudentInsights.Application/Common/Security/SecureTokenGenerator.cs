using System.Security.Cryptography;
using System.Text;

namespace StudentInsights.Application.Common.Security;

/// <summary>
/// Generates opaque, single-use security tokens (email confirmation, password
/// reset, refresh tokens) and hashes them for storage. Raw tokens are only
/// ever handed to the caller (to email or return to the client) and are never
/// persisted — only their SHA-256 hash is stored, matching RefreshToken.TokenHash.
/// </summary>
public static class SecureTokenGenerator
{
    public static string GenerateToken(int byteLength = 32)
    {
        var bytes = RandomNumberGenerator.GetBytes(byteLength);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    public static string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}