namespace StudentInsights.Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendEmailConfirmationAsync(
        string toEmail,
        string firstName,
        Guid userId,
        string confirmationToken,
        CancellationToken cancellationToken = default);

    Task SendPasswordResetAsync(
        string toEmail,
        string firstName,
        string resetToken,
        CancellationToken cancellationToken = default);
}