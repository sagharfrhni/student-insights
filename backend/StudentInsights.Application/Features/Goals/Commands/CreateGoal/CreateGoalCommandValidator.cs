using FluentValidation;
using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.Goals.Commands.CreateGoal;

/// <summary>
/// Mirrors the exact RelatedActivityId/Type invariant Goal.Create() itself
/// enforces (it throws DomainException when the two disagree), same
/// reasoning as CreatePersonalEventCommandValidator mirroring
/// PersonalEvent.Create()'s EndAtUtc check -- a bad request fails fast as
/// a 400 with a structured per-field error instead of falling through to
/// the entity's DomainException.
/// </summary>
public class CreateGoalCommandValidator : AbstractValidator<CreateGoalCommand>
{
    public CreateGoalCommandValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("A valid goal type is required.");

        RuleFor(x => x.TargetValue)
            .GreaterThan(0).WithMessage("Target value must be greater than zero.");

        RuleFor(x => x.RelatedActivityId)
            .NotNull().WithMessage("RelatedActivityId is required for ProjectDeadline goals.")
            .When(x => x.Type == GoalType.ProjectDeadline);

        RuleFor(x => x.RelatedActivityId)
            .Null().WithMessage("RelatedActivityId can only be set for ProjectDeadline goals.")
            .When(x => x.Type != GoalType.ProjectDeadline);
    }
}