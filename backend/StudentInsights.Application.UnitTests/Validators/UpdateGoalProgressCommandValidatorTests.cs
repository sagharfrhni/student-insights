using FluentValidation.TestHelper;
using StudentInsights.Application.Features.Goals.Commands.UpdateGoalProgress;

namespace StudentInsights.UnitTests.Validators;

public class UpdateGoalProgressCommandValidatorTests
{
    private readonly UpdateGoalProgressCommandValidator _validator = new();

    private static UpdateGoalProgressCommand ValidCommand() =>
        new(Guid.NewGuid(), 5m);

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyGoalId_HasError()
    {
        var command = ValidCommand() with { GoalId = Guid.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.GoalId);
    }

    [Fact]
    public void Validate_NegativeCurrentValue_HasError()
    {
        var command = ValidCommand() with { CurrentValue = -1m };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CurrentValue);
    }

    [Fact]
    public void Validate_ZeroCurrentValue_HasNoError()
    {
        // Zero is a legitimate "no progress yet" value — only negative
        // values are rejected.
        var command = ValidCommand() with { CurrentValue = 0m };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.CurrentValue);
    }
}