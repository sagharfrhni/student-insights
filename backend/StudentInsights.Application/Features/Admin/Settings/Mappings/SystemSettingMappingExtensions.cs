using StudentInsights.Application.Features.Admin.Settings.DTOs;
using StudentInsights.Domain.Entities;

namespace StudentInsights.Application.Features.Admin.Settings.Mappings;

public static class SystemSettingMappingExtensions
{
    public static SystemSettingDto ToDto(this SystemSetting setting)
    {
        return new SystemSettingDto(
            setting.Id,
            setting.Key,
            setting.Value,
            setting.Description,
            setting.CreatedAtUtc,
            setting.UpdatedAtUtc);
    }
}