using MediatR;
using StudentInsights.Application.Features.Admin.Settings.DTOs;

namespace StudentInsights.Application.Features.Admin.Settings.Commands.UpdateSystemSettingValue;

public record UpdateSystemSettingValueCommand(
    string Key,
    string Value) : IRequest<SystemSettingDto>;