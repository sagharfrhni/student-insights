using FluentValidation;

namespace StudentInsights.Application.Features.Goals.Commands.UpdateGoalProgress;

/// <summary>
/// Field-shape validation only. Whether the goal's type even allows a
/// manual progress update is a stateful/business rule, so it belongs in
/// UpdateGoalProgressCommandHandler, not here -- same "Handler owns
/// cross-field/stateful rules, Validator owns field shape" split used for
/// UpdateLearningActivityStatusCommandValidator.
/// </summary>
public class UpdateGoalProgressCommandValidator : AbstractValidator<UpdateGoalProgressCommand>
{
    public UpdateGoalProgressCommandValidator()
    {
        RuleFor(x => x.GoalId)
            .NotEmpty().WithMessage("GoalId is required.");

        RuleFor(x => x.CurrentValue)
            .GreaterThanOrEqualTo(0).WithMessage("Current value cannot be negative.");
    }
}