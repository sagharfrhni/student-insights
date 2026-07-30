using FluentValidation.Results;

namespace StudentInsights.Application.Common.Exceptions;

/// <summary>
/// Thrown by ValidationBehavior when a command/query fails FluentValidation.
/// Kept in the same namespace as NotFoundException/ForbiddenAccessException
/// so ExceptionHandlingMiddleware has one place to catch every Application-
/// layer exception type. Carries a PropertyName -&gt; errors[] dictionary so
/// the 400 response can point the client at exactly which fields failed.
/// </summary>
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(failure => failure.PropertyName, failure => failure.ErrorMessage)
            .ToDictionary(group => group.Key, group => group.ToArray());
    }
}