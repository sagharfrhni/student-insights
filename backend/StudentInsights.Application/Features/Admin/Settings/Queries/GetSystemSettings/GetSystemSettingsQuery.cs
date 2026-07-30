using MediatR;
using StudentInsights.Application.Features.Admin.Settings.DTOs;

namespace StudentInsights.Application.Features.Admin.Settings.Queries.GetSystemSettings;

public record GetSystemSettingsQuery : IRequest<IReadOnlyList<SystemSettingDto>>;