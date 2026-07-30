using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Features.Admin.Settings.DTOs;
using StudentInsights.Application.Features.Admin.Settings.Mappings;
using StudentInsights.Domain.Entities;
using ValidationException = StudentInsights.Application.Common.Exceptions.ValidationException;

namespace StudentInsights.Application.Features.Admin.Settings.Commands.CreateSystemSetting;

public class CreateSystemSettingCommandHandler
    : IRequestHandler<CreateSystemSettingCommand, SystemSettingDto>
{
    private readonly IApplicationDbContext _context;

    public CreateSystemSettingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SystemSettingDto> Handle(
        CreateSystemSettingCommand request,
        CancellationToken cancellationToken)
    {
        var keyExists = await _context.SystemSettings
            .AnyAsync(s => s.Key == request.Key, cancellationToken);

        if (keyExists)
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(request.Key), "A system setting with this key already exists.")
            });

        var setting = SystemSetting.Create(request.Key, request.Value, request.Description);

        _context.SystemSettings.Add(setting);

        await _context.SaveChangesAsync(cancellationToken);

        return setting.ToDto();
    }
}