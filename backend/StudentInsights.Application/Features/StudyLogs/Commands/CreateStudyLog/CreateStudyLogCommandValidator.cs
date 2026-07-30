// StudentInsights.Application/Features/StudyLogs/Commands/CreateStudyLog/CreateStudyLogCommandValidator.cs
using FluentValidation;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.StudyLogs.Commands.CreateStudyLog;

/// <summary>
/// Field-shape validation only. Duration bounds mirror StudyLog's own
/// MaxDurationMinutes invariant (the same "validator + Domain invariant"
/// split CreateExamCommandValidator uses for Title/Description length),
/// and Notes' length limit mirrors StudyLogConfiguration's actual
/// HasMaxLength(2000). Course ownership is NOT checked here — it requires
/// a database round-trip, so it's enforced in
/// CreateStudyLogCommandHandler instead, per the project's convention
/// that FluentValidation validates input shape, not cross-entity state.
/// Unlike CreateExamCommandValidator, there is no duplicate-session check:
/// the roadmap explicitly allows multiple StudyLog entries for the same
/// course on the same day.
/// </summary>
public class CreateStudyLogCommandValidator : AbstractValidator<CreateStudyLogCommand>
{
    public CreateStudyLogCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("CourseId is required.");

        // LessThanOrEqualTo takes a Func<T, TProperty> here rather than a
        // fixed DateTime.UtcNow value — the latter would be captured once
        // when the validator instance is constructed, not re-evaluated at
        // the moment validation actually runs (same reasoning as
        // CreateExamCommandValidator.ExamDateUtc).
        RuleFor(x => x.StudyDateUtc)
            .NotEmpty().WithMessage("Study date is required.")
            .LessThanOrEqualTo(x => DateTime.UtcNow).WithMessage("Study date cannot be in the future.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("Duration must be greater than zero minutes.")
            .LessThanOrEqualTo(StudyLog.MaxDurationMinutes)
                .WithMessage($"Duration must not exceed {StudyLog.MaxDurationMinutes} minutes.");

        // No .When(Notes is not null) guard needed: FluentValidation's
        // MaximumLength already treats a null value as valid and only
        // checks length when a value is present.
        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters.");
    }
}