using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Admin.Users.Common;
using StudentInsights.Application.Features.Admin.Users.DTOs;
using StudentInsights.Application.Features.Admin.Users.Mappings;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.Admin.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, AdminUserDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetUserByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminUserDetailDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
            throw new NotFoundException(nameof(User), request.UserId);

        var counts = await AdminUserActivityCounts.GetAsync(_context, user.Id, cancellationToken);

        return user.ToDetailDto(counts.CourseCount, counts.LearningActivityCount, counts.ExamCount, counts.GoalCount);
    }
}