using MediatR;
using StudentInsights.Application.Features.Admin.Settings.DTOs;

namespace StudentInsights.Application.Features.Admin.Settings.Commands.CreateSystemSetting;

public record CreateSystemSettingCommand(
    string Key,
    string Value,
    string? Description) : IRequest<SystemSettingDto>;