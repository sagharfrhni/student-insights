using MediatR;
using StudentInsights.Application.Features.Analytics.DTOs;

namespace StudentInsights.Application.Features.Analytics.Queries.GetWeeklyActivity;

/// <summary>
/// WeekStartDate is optional and may be any day within the desired week --
/// the handler resolves it to that week's actual start via
/// WeekBoundary.GetUtcWeekStart, the same single source of truth
/// Dashboard's weekly study figure already uses, so the two can never
/// disagree about what "this week" means. Omitting it defaults to the
/// week containing DateTime.UtcNow. No validator: unlike a From/To range,
/// there is no invalid state for a single optional DateTime to be in.
/// </summary>
public record GetWeeklyActivityQuery(DateTime? WeekStartDate = null) : IRequest<WeeklyActivityDto>;