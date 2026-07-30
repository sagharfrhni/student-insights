using FluentValidation;

namespace StudentInsights.Application.Features.Admin.Users.Commands.ChangeUserRole;

public class ChangeUserRoleCommandValidator : AbstractValidator<ChangeUserRoleCommand>
{
    public ChangeUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.NewRole)
            .IsInEnum().WithMessage("NewRole must be a valid value.");
    }
}