using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Calendar.DTOs;
using StudentInsights.Application.Features.Calendar.Enums;
using StudentInsights.Application.Features.Calendar.Mappings;

namespace StudentInsights.Application.Features.Calendar.Queries.GetCalendarEvents;

/// <summary>
/// Composes up to five independent, user-scoped, AsNoTracking queries —
/// one per Calendar source — and merges the results into a single
/// chronologically sorted list. Calendar owns no entity of its own (see
/// the module's architectural classification), so this handler is the
/// entirety of its logic: there is no repository or domain service to
/// delegate to.
///
/// Queries run sequentially, not via Task.WhenAll, because
/// IApplicationDbContext is a single scoped DbContext instance and is not
/// safe for concurrent use — see the roadmap deviation noted alongside
/// this module's design review.
/// </summary>
public class GetCalendarEventsQueryHandler
    : IRequestHandler<GetCalendarEventsQuery, IReadOnlyList<CalendarEventDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetCalendarEventsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<CalendarEventDto>> Handle(
        GetCalendarEventsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        // null = every type included (Section 5's "Types omitted -> all
        // five types" default), matching the same "null = unfiltered"
        // convention GetExamsQuery already uses for CourseId/From/To. A
        // non-empty set lets IsRequested skip querying a source entirely
        // when its type wasn't asked for — a real round-trip saved, not
        // just a post-hoc filter on an already-fetched list.
        var requestedTypes = request.Types is { Count: > 0 }
            ? new HashSet<CalendarEventType>(request.Types)
            : null;

        bool IsRequested(CalendarEventType type) => requestedTypes is null || requestedTypes.Contains(type);

        var events = new List<CalendarEventDto>();

        if (IsRequested(CalendarEventType.Class))
        {
            // Include(Course) is required here for the same reason
            // GetExamsQueryHandler includes it: CourseName has to be
            // composed into the title, and Course's own soft-delete query
            // filter is applied automatically wherever Course
            // participates in the query, including through this join —
            // no separate IsDeleted check is needed.
            var classSchedules = await _context.ClassSchedules
                .AsNoTracking()
                .Include(cs => cs.Course)
                .Where(cs => cs.Course.UserId == userId)
                .ToListAsync(cancellationToken);

            events.AddRange(classSchedules.SelectMany(cs =>
                cs.ToCalendarEvents(cs.CourseId, cs.Course.Name, request.FromUtc, request.ToUtc)));
        }

        if (IsRequested(CalendarEventType.Exam))
        {
            // No Include(Course): unlike ExamDto, CalendarEventDto's exam
            // title doesn't carry a course name (per the roadmap's own
            // Title examples), so filtering directly on Exam.UserId avoids
            // the join entirely.
            var exams = await _context.Exams
                .AsNoTracking()
                .Where(e =>
                    e.UserId == userId &&
                    e.ExamDateUtc >= request.FromUtc &&
                    e.ExamDateUtc <= request.ToUtc)
                .ToListAsync(cancellationToken);

            events.AddRange(exams.Select(e => e.ToCalendarEvent()));
        }

        if (IsRequested(CalendarEventType.Deadline))
        {
            var activities = await _context.LearningActivities
                .AsNoTracking()
                .Where(la =>
                    la.UserId == userId &&
                    la.DueDateUtc >= request.FromUtc &&
                    la.DueDateUtc <= request.ToUtc)
                .ToListAsync(cancellationToken);

            events.AddRange(activities.Select(la => la.ToCalendarEvent()));
        }

        if (IsRequested(CalendarEventType.Goal))
        {
            // TargetDateUtc != null enforces Section 2/3's "only dated
            // goals appear on Calendar" rule at the database level, not
            // just in the mapper — goals without a target date (e.g.
            // ongoing GPA targets) never leave the database.
            var goals = await _context.Goals
                .AsNoTracking()
                .Where(g =>
                    g.UserId == userId &&
                    g.TargetDateUtc != null &&
                    g.TargetDateUtc >= request.FromUtc &&
                    g.TargetDateUtc <= request.ToUtc)
                .ToListAsync(cancellationToken);

            events.AddRange(goals.Select(g => g.ToCalendarEvent()));
        }

        if (IsRequested(CalendarEventType.Personal))
        {
            // Overlap filter, not a single-column BETWEEN: PersonalEvent
            // is a genuine [StartAtUtc, EndAtUtc] range (see
            // CalendarEventMappingExtensions.ToCalendarEvent(PersonalEvent)),
            // so an event that merely spans into the requested range must
            // still be included.
            var personalEvents = await _context.PersonalEvents
                .AsNoTracking()
                .Where(pe =>
                    pe.UserId == userId &&
                    pe.StartAtUtc <= request.ToUtc &&
                    pe.EndAtUtc >= request.FromUtc)
                .ToListAsync(cancellationToken);

            events.AddRange(personalEvents.Select(pe => pe.ToCalendarEvent()));
        }

        // Single sort after merge, not per-source — sorting five small
        // pre-sorted-by-nothing lists individually would be wasted work,
        // per Section 4's "Sorting" guidance.
        return events
            .OrderBy(e => e.StartAtUtc)
            .ToList();
    }
}