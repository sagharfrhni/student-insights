using MediatR;
using StudentInsights.Application.Features.Analytics.DTOs;
using StudentInsights.Application.Features.Analytics.Enums;

namespace StudentInsights.Application.Features.Analytics.Queries.GetStudyTime;

/// <summary>
/// Granularity is nullable, not defaulted, so that omitting it is a
/// distinct, rejectable state -- GetStudyTimeQueryValidator enforces it's
/// supplied, same as the roadmap's "granularity (required)" rule. From/To
/// are optional and, like every other Analytics endpoint, "all-time" when
/// both are omitted rather than some hidden default window.
/// </summary>
public record GetStudyTimeQuery(
    StudyTimeGranularity? Granularity,
    DateTime? From = null,
    DateTime? To = null) : IRequest<StudyTimeDto>;