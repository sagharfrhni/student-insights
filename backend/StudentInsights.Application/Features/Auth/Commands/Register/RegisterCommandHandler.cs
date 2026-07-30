using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Common.Security;
using StudentInsights.Domain.Common;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResult>
{
    private static readonly TimeSpan EmailConfirmationTokenLifetime = TimeSpan.FromHours(24);

    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailSender _emailSender;

    public RegisterCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IEmailSender emailSender)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _emailSender = emailSender;
    }

    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        PasswordPolicy.EnsureValid(request.Password);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailInUse = await _context.Users
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (emailInUse)
            throw new DomainException("An account with this email already exists.");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = User.Create(request.FirstName, request.LastName, request.Email, passwordHash);

        var rawToken = SecureTokenGenerator.GenerateToken();
        var tokenHash = SecureTokenGenerator.Hash(rawToken);
        user.SetEmailConfirmationToken(tokenHash, DateTime.UtcNow.Add(EmailConfirmationTokenLifetime));

        _context.Users.Add(user);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // The AnyAsync check above covers the common case; this catches the
            // narrow race where two registrations for the same email land at
            // nearly the same instant. Re-check rather than assuming the failure
            // was the email-uniqueness violation, so an unrelated DbUpdateException
            // (e.g. a transient DB error) isn't misreported as "email already exists".
            var stillInUse = await _context.Users
                .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

            if (stillInUse)
                throw new DomainException("An account with this email already exists.");

            throw;
        }

        await _emailSender.SendEmailConfirmationAsync(user.Email, user.FirstName, user.Id, rawToken, cancellationToken);

        return new RegisterResult(user.Id, user.Email);
    }
}