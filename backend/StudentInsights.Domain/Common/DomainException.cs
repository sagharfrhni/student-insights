namespace StudentInsights.Domain.Common;

/// <summary>
/// Thrown when a domain invariant is violated. Lets the Application layer
/// distinguish business-rule failures (map to 400/422) from unexpected
/// framework errors (map to 500) in one place, instead of guessing from
/// ArgumentException/NullReferenceException.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}