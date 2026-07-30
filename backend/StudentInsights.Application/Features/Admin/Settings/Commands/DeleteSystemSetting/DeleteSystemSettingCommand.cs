using MediatR;

namespace StudentInsights.Application.Features.Admin.Settings.Commands.DeleteSystemSetting;

public record DeleteSystemSettingCommand(string Key) : IRequest;