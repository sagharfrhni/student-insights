// StudentInsights.Application/Features/StudyLogs/Queries/GetStudyLogById/GetStudyLogByIdQuery.cs
using MediatR;
using StudentInsights.Application.Features.StudyLogs.DTOs;

namespace StudentInsights.Application.Features.StudyLogs.Queries.GetStudyLogById;

public record GetStudyLogByIdQuery(Guid StudyLogId) : IRequest<StudyLogDto>;