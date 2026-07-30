using MediatR;
using StudentInsights.Application.Features.Admin.Settings.DTOs;

namespace StudentInsights.Application.Features.Admin.Settings.Queries.GetSystemSettingByKey;

public record GetSystemSettingByKeyQuery(string Key) : IRequest<SystemSettingDto>;