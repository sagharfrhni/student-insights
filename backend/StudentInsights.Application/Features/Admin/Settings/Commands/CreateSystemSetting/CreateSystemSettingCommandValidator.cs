using FluentValidation;

namespace StudentInsights.Application.Features.Admin.Settings.Commands.CreateSystemSetting;

public class CreateSystemSettingCommandValidator : AbstractValidator<CreateSystemSettingCommand>
{
    public CreateSystemSettingCommandValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Key is required.")
            .MaximumLength(100).WithMessage("Key must not exceed 100 characters.");

        RuleFor(x => x.Value)
            .NotNull().WithMessage("Value is required.")
            .MaximumLength(1000).WithMessage("Value must not exceed 1000 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
    }
}