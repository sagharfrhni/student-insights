using FluentValidation;

namespace StudentInsights.Application.Features.Goals.Commands.UpdateGoal;

/// <summary>Mirrors the exact "target value must be greater than zero" invariant Goal.UpdateTarget() enforces.</summary>
public class UpdateGoalCommandValidator : AbstractValidator<UpdateGoalCommand>
{
    public UpdateGoalCommandValidator()
    {
        RuleFor(x => x.GoalId)
            .NotEmpty().WithMessage("GoalId is required.");

        RuleFor(x => x.TargetValue)
            .GreaterThan(0).WithMessage("Target value must be greater than zero.");
    }
}