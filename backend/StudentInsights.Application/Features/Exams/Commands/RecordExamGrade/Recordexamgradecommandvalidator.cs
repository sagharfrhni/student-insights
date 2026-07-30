using FluentValidation;
using StudentInsights.Domain.ValueObjects;

namespace StudentInsights.Application.Features.Exams.Commands.RecordExamGrade;

/// <summary>
/// Field-shape validation only, mirroring the Grade value object's own
/// 0-20 invariant.
/// Ownership is NOT checked here -- it requires a database round-trip,
/// so it's enforced in RecordExamGradeCommandHandler instead, per the
/// project's convention that FluentValidation validates input shape, not
/// cross-entity state.
/// </summary>
public class RecordExamGradeCommandValidator : AbstractValidator<RecordExamGradeCommand>
{
    public RecordExamGradeCommandValidator()
    {
        RuleFor(x => x.ExamId)
            .NotEmpty()
            .WithMessage("ExamId is required.");

        RuleFor(x => x.Grade)
            .GreaterThanOrEqualTo(Grade.MinValue)
            .LessThanOrEqualTo(Grade.MaxValue)
            .WithMessage($"Grade must be between {Grade.MinValue} and {Grade.MaxValue}.");

        RuleFor(x => x.Grade)
            .Must(grade => decimal.Round(grade, 2) == grade)
            .WithMessage("Grade cannot have more than two decimal places.");
    }
}