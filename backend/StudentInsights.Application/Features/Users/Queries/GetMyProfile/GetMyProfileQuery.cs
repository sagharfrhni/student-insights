using MediatR;
using StudentInsights.Application.Features.Users.DTOs;

namespace StudentInsights.Application.Features.Users.Queries.GetMyProfile;

public record GetMyProfileQuery : IRequest<UserProfileDto>;
