using FluentValidation;

namespace StudentInsights.Application.Features.Notifications.Queries.GetNotifications;

/// <summary>
/// Defense-in-depth only — ASP.NET Core's model binding already rejects
/// an out-of-range Type value before this query reaches the handler, so
/// this rule is technically redundant. Included anyway, cheap and
/// consistent with GetExamsQueryValidator's "validate what has a real
/// invariant" convention; unlike GetExamsQueryValidator's From/To rule,
/// there is no cross-field invariant here to enforce.
/// </summary>
public class GetNotificationsQueryValidator : AbstractValidator<GetNotificationsQuery>
{
    public GetNotificationsQueryValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum()
            .When(x => x.Type.HasValue);
    }
}