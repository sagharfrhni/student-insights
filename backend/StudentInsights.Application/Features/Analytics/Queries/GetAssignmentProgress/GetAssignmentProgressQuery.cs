using MediatR;
using StudentInsights.Application.Features.Analytics.DTOs;

namespace StudentInsights.Application.Features.Analytics.Queries.GetAssignmentProgress;

/// <summary>
/// From/To scope the count by LearningActivity.DueDateUtc and are both
/// optional -- omitting both means "all-time", the same "null =
/// unfiltered" convention GetExamsQuery already uses. No CourseId filter:
/// unlike GetExamsQuery, the roadmap's Assignment Analytics metric is a
/// whole-user breakdown, not a per-course one, and nothing downstream
/// (a chart, not a list) needs a per-course cut in the MVP.
/// </summary>
public record GetAssignmentProgressQuery(
    DateTime? From = null,
    DateTime? To = null) : IRequest<AssignmentProgressDto>;