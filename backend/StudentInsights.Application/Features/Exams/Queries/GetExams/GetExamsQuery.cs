// StudentInsights.Application/Features/Exams/Queries/GetExams/GetExamsQuery.cs
using MediatR;
using StudentInsights.Application.Common.Models;
using StudentInsights.Application.Features.Exams.DTOs;

namespace StudentInsights.Application.Features.Exams.Queries.GetExams;

/// <summary>
/// CourseId/From/To are all optional so this same query shape can serve
/// both "all my exams" and the narrower filters the future Calendar/
/// Dashboard features will need, without requiring a second query later.
/// A CourseId that doesn't belong to the current user simply yields an
/// empty page (the base filter below already scopes to Exam.UserId) —
/// not a 403/404, since a list endpoint filtered by a foreign id is not a
/// security signal worth surfacing as an error.
/// </summary>
public record GetExamsQuery(
    PaginationParams Pagination,
    Guid? CourseId = null,
    DateTime? From = null,
    DateTime? To = null) : IRequest<PaginatedResult<ExamDto>>;