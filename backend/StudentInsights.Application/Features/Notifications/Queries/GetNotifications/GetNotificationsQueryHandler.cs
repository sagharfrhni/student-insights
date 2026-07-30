using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Common.Models;
using StudentInsights.Application.Features.Notifications.DTOs;
using StudentInsights.Application.Features.Notifications.Mappings;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.Notifications.Queries.GetNotifications;

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, PaginatedResult<NotificationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetNotificationsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedResult<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == _currentUserService.UserId);

        if (request.IsRead.HasValue)
            query = query.Where(n => n.IsRead == request.IsRead.Value);

        if (request.Type.HasValue)
            query = query.Where(n => n.Type == request.Type.Value);

        // Newest first — a feed of generated items reads newest-first,
        // same reasoning GetGoalsQueryHandler gives for CreatedAtUtc
        // descending (unlike Exams, which orders soonest-first because
        // exams are calendar events, not a feed).
        query = query.OrderByDescending(n => n.CreatedAtUtc);

        var pagedNotifications = await PaginatedResult<Notification>.CreateAsync(
            query,
            request.Pagination.PageNumber,
            request.Pagination.PageSize,
            cancellationToken);

        // Every NotificationDto field is a direct scalar/enum on
        // Notification itself (no cross-aggregate data to denormalize,
        // unlike LearningActivityDto.CourseName), so mapping the already-
        // materialized page via the reusable ToDto() extension — the same
        // pattern GetGoalsQueryHandler uses — is both simpler and avoids
        // duplicating the field list in an inline Select() projection.
        return pagedNotifications.Map(n => n.ToDto());
    }
}