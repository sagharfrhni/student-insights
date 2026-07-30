// StudentInsights.Application/Features/Exams/Queries/GetExamById/GetExamByIdQuery.cs
using MediatR;
using StudentInsights.Application.Features.Exams.DTOs;

namespace StudentInsights.Application.Features.Exams.Queries.GetExamById;

public record GetExamByIdQuery(Guid ExamId) : IRequest<ExamDto>;