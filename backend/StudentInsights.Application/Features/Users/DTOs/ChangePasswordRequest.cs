namespace StudentInsights.Application.Features.Users.DTOs;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
