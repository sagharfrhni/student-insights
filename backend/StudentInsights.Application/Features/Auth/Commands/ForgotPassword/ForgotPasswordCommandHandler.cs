using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Common.Security;

namespace StudentInsights.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
{
    private static readonly TimeSpan PasswordResetTokenLifetime = TimeSpan.FromHours(1);

    private readonly IApplicationDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        IApplicationDbContext context,
        IEmailSender emailSender,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _context = context;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user is null)
            return; // don't reveal whether the email is registered

        var rawToken = SecureTokenGenerator.GenerateToken();
        var tokenHash = SecureTokenGenerator.Hash(rawToken);
        user.SetPasswordResetToken(tokenHash, DateTime.UtcNow.Add(PasswordResetTokenLifetime));

        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            await _emailSender.SendPasswordResetAsync(user.Email, user.FirstName, rawToken, cancellationToken);
        }
        catch (Exception ex)
        {
            // The reset token above is already committed. A mail-provider
            // outage/timeout must not surface as a 500 here -- that would
            // visibly distinguish "email exists but sending failed" from the
            // deliberately generic "unknown email" case above (an early
            // return with the exact same 204 response), defeating the whole
            // point of that generic response.
            _logger.LogError(ex, "Failed to send password reset email to user '{UserId}'.", user.Id);
        }
    }
}