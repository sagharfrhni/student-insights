using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Admin.Settings.DTOs;
using StudentInsights.Application.Features.Admin.Settings.Mappings;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.Admin.Settings.Commands.UpdateSystemSettingValue;

public class UpdateSystemSettingValueCommandHandler
    : IRequestHandler<UpdateSystemSettingValueCommand, SystemSettingDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateSystemSettingValueCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SystemSettingDto> Handle(
        UpdateSystemSettingValueCommand request,
        CancellationToken cancellationToken)
    {
        var setting = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == request.Key, cancellationToken);

        if (setting is null)
            throw new NotFoundException(nameof(SystemSetting), request.Key);

        setting.UpdateValue(request.Value);

        await _context.SaveChangesAsync(cancellationToken);

        return setting.ToDto();
    }
}