// StudentInsights.Application/Features/Exams/Commands/UpdateExam/UpdateExamCommandValidator.cs
using FluentValidation;

namespace StudentInsights.Application.Features.Exams.Commands.UpdateExam;

/// <summary>
/// Same field rules as CreateExamCommandValidator, minus the
/// "cannot be in the past" check — a student editing a past exam (e.g.
/// fixing a typo) should not be blocked, per the roadmap's Update rules.
/// </summary>
public class UpdateExamCommandValidator : AbstractValidator<UpdateExamCommand>
{
    public UpdateExamCommandValidator()
    {
        RuleFor(x => x.ExamId)
            .NotEmpty().WithMessage("ExamId is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.ExamDateUtc)
            .NotEmpty().WithMessage("Exam date is required.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");
    }
}