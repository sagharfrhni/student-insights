
namespace StudentInsights.Application.Common.Interfaces;

/// <summary>
/// Exposes the identity of the currently authenticated user to the
/// Application layer, without that layer taking a dependency on
/// HttpContext. The concrete implementation lives in WebApi (the only
/// project that knows about HTTP) and reads the UserId out of the JWT
/// claims already validated by the authentication middleware.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Id of the authenticated user making the current request.
    /// Every consumer runs behind [Authorize], so a valid value is always
    /// expected to be available; the implementation throws if it is not.
    /// </summary>
    Guid UserId { get; }
}