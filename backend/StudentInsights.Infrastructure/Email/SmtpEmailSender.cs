using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using StudentInsights.Application.Common.Interfaces;

namespace StudentInsights.Infrastructure.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;

    public SmtpEmailSender(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public Task SendEmailConfirmationAsync(
        string toEmail, string firstName, Guid userId, string confirmationToken, CancellationToken cancellationToken = default)
    {
        var link =
            $"{_settings.ClientBaseUrl}/api/auth/confirm-email?userId={userId}&token={Uri.EscapeDataString(confirmationToken)}";
        var subject = "Confirm your StudentInsights account";
        var body = $"Hi {firstName},\n\nPlease confirm your account by visiting the link below:\n{link}\n\nThis link expires in 24 hours.";

        return SendAsync(toEmail, subject, body, cancellationToken);
    }

    public Task SendPasswordResetAsync(
        string toEmail, string firstName, string resetToken, CancellationToken cancellationToken = default)
    {
        var link = $"{_settings.ClientBaseUrl}/reset-password?email={Uri.EscapeDataString(toEmail)}&token={Uri.EscapeDataString(resetToken)}";
        var subject = "Reset your StudentInsights password";
        var body = $"Hi {firstName},\n\nYou requested a password reset. Visit the link below to choose a new password:\n{link}\n\nThis link expires in 1 hour. If you didn't request this, you can ignore this email.";

        return SendAsync(toEmail, subject, body, cancellationToken);
    }

    private async Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromAddress, _settings.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        message.To.Add(toEmail);

        using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
        {
            Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword),
            EnableSsl = true
        };

        await client.SendMailAsync(message, cancellationToken);
    }
}