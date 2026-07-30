using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Admin.Settings.DTOs;
using StudentInsights.Application.Features.Admin.Settings.Mappings;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.Admin.Settings.Queries.GetSystemSettingByKey;

public class GetSystemSettingByKeyQueryHandler
    : IRequestHandler<GetSystemSettingByKeyQuery, SystemSettingDto>
{
    private readonly IApplicationDbContext _context;

    public GetSystemSettingByKeyQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SystemSettingDto> Handle(
        GetSystemSettingByKeyQuery request,
        CancellationToken cancellationToken)
    {
        var setting = await _context.SystemSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Key == request.Key, cancellationToken);

        if (setting is null)
            throw new NotFoundException(nameof(SystemSetting), request.Key);

        return setting.ToDto();
    }
}