using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Exceptions;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.Admin.Settings.Commands.DeleteSystemSetting;

public class DeleteSystemSettingCommandHandler : IRequestHandler<DeleteSystemSettingCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteSystemSettingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteSystemSettingCommand request, CancellationToken cancellationToken)
    {
        var setting = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == request.Key, cancellationToken);

        if (setting is null)
            throw new NotFoundException(nameof(SystemSetting), request.Key);

        _context.SystemSettings.Remove(setting);

        await _context.SaveChangesAsync(cancellationToken);
    }
}