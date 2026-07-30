// StudentInsights.Application/Features/StudyLogs/Commands/UpdateStudyLog/UpdateStudyLogCommandValidator.cs
using FluentValidation;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.StudyLogs.Commands.UpdateStudyLog;

/// <summary>
/// Same field rules as CreateStudyLogCommandValidator, minus the
/// "cannot be in the future" check — a student correcting a previously
/// logged session (e.g. fixing the date or duration) should not be
/// blocked, same reasoning as UpdateExamCommandValidator dropping the
/// "cannot be in the past" check present on Create.
/// </summary>
public class UpdateStudyLogCommandValidator : AbstractValidator<UpdateStudyLogCommand>
{
    public UpdateStudyLogCommandValidator()
    {
        RuleFor(x => x.StudyLogId)
            .NotEmpty().WithMessage("StudyLogId is required.");

        RuleFor(x => x.StudyDateUtc)
            .NotEmpty().WithMessage("Study date is required.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("Duration must be greater than zero minutes.")
            .LessThanOrEqualTo(StudyLog.MaxDurationMinutes)
                .WithMessage($"Duration must not exceed {StudyLog.MaxDurationMinutes} minutes.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters.");
    }
}