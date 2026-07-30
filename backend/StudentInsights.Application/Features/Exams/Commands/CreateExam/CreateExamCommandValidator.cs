// StudentInsights.Application/Features/Exams/Commands/CreateExam/CreateExamCommandValidator.cs
using FluentValidation;

namespace StudentInsights.Application.Features.Exams.Commands.CreateExam;

/// <summary>
/// Field-shape validation only. Title/Description length limits mirror
/// ExamConfiguration's actual HasMaxLength(200)/HasMaxLength(2000), the
/// same way CreateCourseCommandValidator mirrors CourseConfiguration.
/// Ownership and the duplicate-exam rule are NOT checked here — both
/// require a database round-trip, so they're enforced in
/// CreateExamCommandHandler instead, per the project's convention that
/// FluentValidation validates input shape, not cross-entity state.
/// </summary>
public class CreateExamCommandValidator : AbstractValidator<CreateExamCommand>
{
    public CreateExamCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("CourseId is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        // GreaterThanOrEqualTo takes a Func<T, TProperty> here rather than
        // a fixed DateTime.UtcNow value — the latter would be captured
        // once when the validator instance is constructed, not
        // re-evaluated at the moment validation actually runs.
        RuleFor(x => x.ExamDateUtc)
            .NotEmpty().WithMessage("Exam date is required.")
            .GreaterThanOrEqualTo(x => DateTime.UtcNow).WithMessage("Exam date cannot be in the past.");

        // No .When(Description is not null) guard needed: FluentValidation's
        // MaximumLength already treats a null value as valid and only checks
        // length when a value is present.
        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");
    }
}