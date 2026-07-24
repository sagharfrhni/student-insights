using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using StudentInsights.Application.Common.Interfaces;

namespace StudentInsights.WebApi.Services;

/// <summary>
/// Reads the current user's Id from the JWT claims on HttpContext.User.
/// JwtTokenGenerator stores the Id under JwtRegisteredClaimNames.Sub, and
/// because Program.cs does not disable ASP.NET Core's default inbound
/// claim mapping, JwtSecurityTokenHandler remaps "sub" to
/// ClaimTypes.NameIdentifier by the time it reaches HttpContext.User.
/// Both are checked so this keeps working even if that mapping behavior
/// is ever turned off.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            var userIdValue = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrWhiteSpace(userIdValue) || !Guid.TryParse(userIdValue, out var userId))
                throw new UnauthorizedAccessException("Current user could not be resolved from the request.");

            return userId;
        }
    }
}