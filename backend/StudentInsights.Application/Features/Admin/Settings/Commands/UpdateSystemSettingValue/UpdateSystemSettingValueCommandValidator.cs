using FluentValidation;

namespace StudentInsights.Application.Features.Admin.Settings.Commands.UpdateSystemSettingValue;

public class UpdateSystemSettingValueCommandValidator : AbstractValidator<UpdateSystemSettingValueCommand>
{
    public UpdateSystemSettingValueCommandValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Key is required.");

        RuleFor(x => x.Value)
            .NotNull().WithMessage("Value is required.")
            .MaximumLength(1000).WithMessage("Value must not exceed 1000 characters.");
    }
}