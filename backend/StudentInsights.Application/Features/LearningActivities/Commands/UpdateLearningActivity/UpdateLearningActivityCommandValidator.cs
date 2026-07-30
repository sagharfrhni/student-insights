using FluentValidation;

namespace StudentInsights.Application.Features.LearningActivities.Commands.UpdateLearningActivity;

/// <summary>Same field-length rules as CreateLearningActivityCommandValidator, plus LearningActivityId.</summary>
public class UpdateLearningActivityCommandValidator : AbstractValidator<UpdateLearningActivityCommand>
{
    public UpdateLearningActivityCommandValidator()
    {
        RuleFor(x => x.LearningActivityId)
            .NotEmpty().WithMessage("LearningActivityId is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        // Unlike CreateLearningActivityCommandValidator, there is no
        // "not in the past" rule here — the product spec explicitly lifts
        // that restriction on edit. NotEmpty still catches an unset/default
        // DateTime reaching the handler.
        RuleFor(x => x.DueDateUtc)
            .NotEmpty().WithMessage("Due date is required.");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("A valid priority is required.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.ResourceLink)
            .MaximumLength(500).WithMessage("Resource link must not exceed 500 characters.");
    }
}