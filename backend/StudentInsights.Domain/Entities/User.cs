using System.Text.RegularExpressions;
using StudentInsights.Domain.Common;
using StudentInsights.Domain.Enums;

namespace StudentInsights.Domain.Entities;

public class User : BaseEntity
{
    private static readonly Regex EmailPattern = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled);

    private readonly List<RefreshToken> _refreshTokens = new();
    private readonly List<Course> _courses = new();
    private readonly List<Goal> _goals = new();
    private readonly List<PersonalEvent> _personalEvents = new();
    private readonly List<StudyLog> _studyLogs = new();
    private readonly List<Notification> _notifications = new();
    private readonly List<LearningActivity> _learningActivities = new();
    private readonly List<Exam> _exams = new();

    private User()
    {
    }

    private User(
        string firstName,
        string lastName,
        string email,
        string passwordHash,
        UserRole role)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
    }

    public static User Create(
        string firstName,
        string lastName,
        string email,
        string passwordHash,
        UserRole role = UserRole.Student)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required.");

        if (string.IsNullOrWhiteSpace(email) || !EmailPattern.IsMatch(email.Trim()))
            throw new DomainException("A valid email is required.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.");

        return new User(
            firstName.Trim(),
            lastName.Trim(),
            email.Trim().ToLowerInvariant(),
            passwordHash,
            role);
    }

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; } = UserRole.Student;
    public bool IsActive { get; private set; } = true;
    public bool EmailConfirmed { get; private set; }
    public DateTime? EmailConfirmedAtUtc { get; private set; }

    /// <summary>Hash of the current pending email-confirmation token, if
    /// any. The raw token is never persisted.</summary>
    public string? EmailConfirmationTokenHash { get; private set; }
    public DateTime? EmailConfirmationTokenExpiresAtUtc { get; private set; }

    /// <summary>Hash of the current pending password-reset token, if any.
    /// The raw token is never persisted.</summary>
    public string? PasswordResetTokenHash { get; private set; }
    public DateTime? PasswordResetTokenExpiresAtUtc { get; private set; }

    /// <summary>Single source of truth for whether this user is allowed
    /// to authenticate.</summary>
    //public bool CanLogIn() => IsActive && EmailConfirmed;
    public bool CanLogIn() => IsActive;

    public void SetEmailConfirmationToken(string tokenHash, DateTime expiresAtUtc)
    {
        EnsureValidToken(tokenHash, expiresAtUtc, "Confirmation");
        EmailConfirmationTokenHash = tokenHash;
        EmailConfirmationTokenExpiresAtUtc = expiresAtUtc;
        MarkModified();
    }

    public void ConfirmEmail(string tokenHash)
    {
        if (EmailConfirmed)
            return;

        if (string.IsNullOrWhiteSpace(EmailConfirmationTokenHash) ||
            EmailConfirmationTokenHash != tokenHash)
            throw new DomainException("Invalid confirmation token.");

        if (EmailConfirmationTokenExpiresAtUtc is null ||
            EmailConfirmationTokenExpiresAtUtc <= DateTime.UtcNow)
            throw new DomainException("Confirmation token has expired.");

        EmailConfirmed = true;
        EmailConfirmedAtUtc = DateTime.UtcNow;
        EmailConfirmationTokenHash = null;
        EmailConfirmationTokenExpiresAtUtc = null;
        MarkModified();
    }

    public void SetPasswordResetToken(string tokenHash, DateTime expiresAtUtc)
    {
        EnsureValidToken(tokenHash, expiresAtUtc, "Reset");
        PasswordResetTokenHash = tokenHash;
        PasswordResetTokenExpiresAtUtc = expiresAtUtc;
        MarkModified();
    }

    public void ResetPassword(string tokenHash, string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(PasswordResetTokenHash) ||
            PasswordResetTokenHash != tokenHash)
            throw new DomainException("Invalid password reset token.");

        if (PasswordResetTokenExpiresAtUtc is null ||
            PasswordResetTokenExpiresAtUtc <= DateTime.UtcNow)
            throw new DomainException("Password reset token has expired.");

        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new DomainException("Password hash is required.");

        PasswordHash = newPasswordHash;
        PasswordResetTokenHash = null;
        PasswordResetTokenExpiresAtUtc = null;
        MarkModified();
    }

    /// <summary>Shared validation for the two token-setter methods above --- kept as one
    /// place so the confirmation and reset flows can't drift apart.</summary>
    private static void EnsureValidToken(string tokenHash, DateTime expiresAtUtc, string tokenKind)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new DomainException($"{tokenKind} token hash is required.");

        if (expiresAtUtc <= DateTime.UtcNow)
            throw new DomainException("Expiration must be in the future.");
    }

    public void ChangePasswordHash(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new DomainException("Password hash is required.");

        PasswordHash = newPasswordHash;
        MarkModified();
    }

    public void Rename(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required.");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        MarkModified();
    }

    public void Activate()
    {
        IsActive = true;
        MarkModified();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkModified();
    }

    public void PromoteToAdmin()
    {
        Role = UserRole.Admin;
        MarkModified();
    }

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens;
    public IReadOnlyCollection<Course> Courses => _courses;
    public IReadOnlyCollection<Goal> Goals => _goals;
    public IReadOnlyCollection<PersonalEvent> PersonalEvents => _personalEvents;
    public IReadOnlyCollection<StudyLog> StudyLogs => _studyLogs;
    public IReadOnlyCollection<Notification> Notifications => _notifications;
    public IReadOnlyCollection<LearningActivity> LearningActivities => _learningActivities;
    public IReadOnlyCollection<Exam> Exams => _exams;
}