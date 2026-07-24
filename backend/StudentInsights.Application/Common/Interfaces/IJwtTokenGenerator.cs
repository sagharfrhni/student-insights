using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) GenerateAccessToken(User user);
}