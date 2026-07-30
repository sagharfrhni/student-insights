namespace StudentInsights.Application.Features.Admin.Settings.DTOs;

public record CreateSystemSettingRequest(
    string Key,
    string Value,
    string? Description);