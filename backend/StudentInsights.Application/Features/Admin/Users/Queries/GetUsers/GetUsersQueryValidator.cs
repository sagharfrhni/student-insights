using FluentValidation;

namespace StudentInsights.Application.Features.Admin.Users.Queries.GetUsers;

public class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(200).WithMessage("Search term must not exceed 200 characters.");

        RuleFor(x => x.Role)
            .IsInEnum().When(x => x.Role.HasValue)
            .WithMessage("Role must be a valid value.");
    }
}