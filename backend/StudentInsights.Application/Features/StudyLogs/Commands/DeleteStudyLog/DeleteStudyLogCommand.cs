// StudentInsights.Application/Features/StudyLogs/Commands/DeleteStudyLog/DeleteStudyLogCommand.cs
using MediatR;

namespace StudentInsights.Application.Features.StudyLogs.Commands.DeleteStudyLog;

public record DeleteStudyLogCommand(Guid StudyLogId) : IRequest;