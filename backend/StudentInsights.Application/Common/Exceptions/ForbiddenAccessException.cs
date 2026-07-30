namespace StudentInsights.Application.Common.Exceptions;

/// <summary>
/// Thrown by a Handler when the current user is authenticated but is not
/// the owner of the requested resource (e.g. trying to edit another
/// user's Course). ExceptionHandlingMiddleware maps this to 403.
/// </summary>
public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException()
        : base("You do not have permission to perform this action.")
    {
    }

    public ForbiddenAccessException(string message) : base(message)
    {
    }
}