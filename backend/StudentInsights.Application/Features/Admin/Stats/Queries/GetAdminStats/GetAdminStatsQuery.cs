using MediatR;
using StudentInsights.Application.Features.Admin.Stats.DTOs;

namespace StudentInsights.Application.Features.Admin.Stats.Queries.GetAdminStats;

public record GetAdminStatsQuery : IRequest<AdminStatsDto>;