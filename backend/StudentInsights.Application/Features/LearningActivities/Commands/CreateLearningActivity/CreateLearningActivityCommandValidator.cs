using FluentValidation;

namespace StudentInsights.Application.Features.LearningActivities.Commands.CreateLearningActivity;

/// <summary>
/// Mirrors LearningActivityConfiguration's HasMaxLength(200/2000/500) on
/// Title/Description/ResourceLink, so bad input is rejected with a clean
/// 400 here instead of surfacing as a DomainException or a DB truncation
/// error — same reasoning as CreateCourseCommandValidator.
/// </summary>
public class CreateLearningActivityCommandValidator : AbstractValidator<CreateLearningActivityCommand>
{
    public CreateLearningActivityCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("CourseId is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("A valid activity type is required.");

        RuleFor(x => x.DueDateUtc)
            .Must(dueDateUtc => dueDateUtc >= DateTime.UtcNow)
            .WithMessage("Due date cannot be in the past.");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("A valid priority is required.");

        // No .When(Description is not null) guard needed: FluentValidation's
        // MaximumLength already treats a null value as valid and only checks
        // length when a value is present.
        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.ResourceLink)
            .MaximumLength(500).WithMessage("Resource link must not exceed 500 characters.");
    }
}