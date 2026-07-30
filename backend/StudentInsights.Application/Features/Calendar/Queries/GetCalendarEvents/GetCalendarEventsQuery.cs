using MediatR;
using StudentInsights.Application.Features.Calendar.DTOs;
using StudentInsights.Application.Features.Calendar.Enums;

namespace StudentInsights.Application.Features.Calendar.Queries.GetCalendarEvents;

/// <summary>
/// FromUtc/ToUtc use DateTime rather than DateOnly to stay consistent
/// with every other date-bearing query in the project (e.g.
/// GetExamsQuery.From/To) — this is the only place in the codebase where
/// DateOnly would otherwise appear, and both bounds are required (unlike
/// GetExamsQuery's optional From/To) since Calendar always needs a
/// bounded range to avoid unbounded per-source scans across all five
/// sources.
///
/// Types is optional and, when omitted, means "all five types" per
/// Section 5 — the same "null = unfiltered" convention already used by
/// GetExamsQuery.CourseId/From/To.
///
/// No PaginationParams: calendar ranges are inherently bounded by the
/// caller (typically a month) and the max-range check in
/// GetCalendarEventsQueryValidator is the actual abuse guard, so adding
/// PaginatedResult&lt;T&gt; here would be complexity without a real benefit.
/// </summary>
public record GetCalendarEventsQuery(
    DateTime FromUtc,
    DateTime ToUtc,
    IReadOnlyCollection<CalendarEventType>? Types = null) : IRequest<IReadOnlyList<CalendarEventDto>>;