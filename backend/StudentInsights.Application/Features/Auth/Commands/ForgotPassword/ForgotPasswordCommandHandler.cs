using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Common.Security;

namespace StudentInsights.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
{
    private static readonly TimeSpan PasswordResetTokenLifetime = TimeSpan.FromHours(1);

    private readonly IApplicationDbContext _context;
    private readonly IEmailSender _emailSender;

    public ForgotPasswordCommandHandler(IApplicationDbContext context, IEmailSender emailSender)
    {
        _context = context;
        _emailSender = emailSender;
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

        await _emailSender.SendPasswordResetAsync(user.Email, user.FirstName, rawToken, cancellationToken);
    }
}