using FluentValidation;
using MediatR;
using ValidationException = StudentInsights.Application.Common.Exceptions.ValidationException;

namespace StudentInsights.Application.Common.Behaviors;

/// <summary>
/// Runs every registered IValidator&lt;TRequest&gt; before the request reaches
/// its handler, throwing ValidationException on failure. Requests with no
/// registered validator (e.g. LoginCommand, which validates manually via
/// PasswordPolicy) pass straight through — this behavior is additive and
/// doesn't force every existing handler to gain a validator.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}