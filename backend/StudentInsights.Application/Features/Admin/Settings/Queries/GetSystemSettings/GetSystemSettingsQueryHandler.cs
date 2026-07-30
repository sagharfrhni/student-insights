using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Admin.Settings.DTOs;

namespace StudentInsights.Application.Features.Admin.Settings.Queries.GetSystemSettings;

public class GetSystemSettingsQueryHandler
    : IRequestHandler<GetSystemSettingsQuery, IReadOnlyList<SystemSettingDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSystemSettingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SystemSettingDto>> Handle(
        GetSystemSettingsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.SystemSettings
            .AsNoTracking()
            .OrderBy(s => s.Key)
            .Select(s => new SystemSettingDto(
                s.Id,
                s.Key,
                s.Value,
                s.Description,
                s.CreatedAtUtc,
                s.UpdatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}