using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Admin.Users.Common;
using StudentInsights.Application.Features.Admin.Users.DTOs;
using StudentInsights.Application.Features.Admin.Users.Mappings;
using StudentInsights.Domain.Common;
using StudentInsights.Domain.Entities;
using StudentInsights.Domain.Enums;

namespace StudentInsights.Application.Features.Admin.Users.Commands.ChangeUserRole;

public class ChangeUserRoleCommandHandler : IRequestHandler<ChangeUserRoleCommand, AdminUserDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ChangeUserRoleCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<AdminUserDetailDto> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == _currentUserService.UserId && request.NewRole == UserRole.Student)
            throw new DomainException("An administrator cannot demote their own account.");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
            throw new NotFoundException(nameof(User), request.UserId);

        if (request.NewRole == UserRole.Admin)
            user.PromoteToAdmin();
        else
            user.DemoteToStudent();

        await _context.SaveChangesAsync(cancellationToken);

        var counts = await AdminUserActivityCounts.GetAsync(_context, user.Id, cancellationToken);

        return user.ToDetailDto(counts.CourseCount, counts.LearningActivityCount, counts.ExamCount, counts.GoalCount);
    }
}