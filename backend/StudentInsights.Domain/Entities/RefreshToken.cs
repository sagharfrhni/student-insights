using StudentInsights.Domain.Common;

namespace StudentInsights.Domain.Entities;

public class RefreshToken : BaseEntity
{
    private RefreshToken()
    {
    } // EF Core

    private RefreshToken(Guid userId, string tokenHash, DateTime expiresAtUtc, string? createdByIp)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        CreatedByIp = createdByIp;
    }

    public static RefreshToken Create(User user, string tokenHash, DateTime expiresAtUtc, string? createdByIp = null)
    {
        if (user is null)
            throw new DomainException("User is required.");
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new DomainException("Token hash is required.");
        if (expiresAtUtc <= DateTime.UtcNow)
            throw new DomainException("Expiration must be in the future.");

        return new RefreshToken(user.Id, tokenHash, expiresAtUtc, createdByIp);
    }

    public Guid UserId { get; private set; }

    public User User { get; private set; } = null!;

    /// <summary>Hashed value of the token; the raw token is never persisted.</summary>
    public string TokenHash { get; private set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime? RevokedAtUtc { get; private set; }

    public string? CreatedByIp { get; private set; }

    public string? RevokedByIp { get; private set; }

    /// <summary>Hash of the token this one was rotated into, if it was rotated rather than plainly revoked. Used to detect reuse of a stolen/rotated token.</summary>
    public string? ReplacedByTokenHash { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;

    public bool IsRevoked => RevokedAtUtc.HasValue;

    public bool IsActive => !IsExpired && !IsRevoked;

    public void Revoke(string? revokedByIp = null, string? replacedByTokenHash = null)
    {
        if (IsRevoked) return;
        RevokedAtUtc = DateTime.UtcNow;
        RevokedByIp = revokedByIp;
        ReplacedByTokenHash = replacedByTokenHash;
        MarkModified();
    }
}