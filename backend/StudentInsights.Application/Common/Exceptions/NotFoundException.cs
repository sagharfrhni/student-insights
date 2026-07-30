namespace StudentInsights.Application.Common.Exceptions;

/// <summary>
/// Thrown by a Handler when a requested entity does not exist (or is not
/// visible to the current user, e.g. soft-deleted and filtered out by the
/// global query filter). ExceptionHandlingMiddleware maps this to 404.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string entityName, object key)
        : base($"Entity \"{entityName}\" ({key}) was not found.")
    {
    }
}