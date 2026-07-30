namespace StudentInsights.Application.Features.Admin.Settings.DTOs;

public record SystemSettingDto(
    Guid Id,
    string Key,
    string Value,
    string? Description,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);