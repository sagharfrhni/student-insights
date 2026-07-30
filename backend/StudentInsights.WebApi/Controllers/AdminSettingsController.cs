using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentInsights.Application.Features.Admin.Settings.Commands.CreateSystemSetting;
using StudentInsights.Application.Features.Admin.Settings.Commands.DeleteSystemSetting;
using StudentInsights.Application.Features.Admin.Settings.Commands.UpdateSystemSettingValue;
using StudentInsights.Application.Features.Admin.Settings.DTOs;
using StudentInsights.Application.Features.Admin.Settings.Queries.GetSystemSettingByKey;
using StudentInsights.Application.Features.Admin.Settings.Queries.GetSystemSettings;

namespace StudentInsights.WebApi.Controllers;

[ApiController]
[Route("api/admin/settings")]
[Authorize(Roles = "Admin")]
public class AdminSettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminSettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SystemSettingDto>>> GetSystemSettings(
        CancellationToken cancellationToken)
    {
        var settings = await _mediator.Send(new GetSystemSettingsQuery(), cancellationToken);
        return Ok(settings);
    }

    [HttpGet("{key}")]
    public async Task<ActionResult<SystemSettingDto>> GetSystemSettingByKey(
        string key,
        CancellationToken cancellationToken)
    {
        var setting = await _mediator.Send(new GetSystemSettingByKeyQuery(key), cancellationToken);
        return Ok(setting);
    }

    [HttpPost]
    public async Task<ActionResult<SystemSettingDto>> CreateSystemSetting(
        [FromBody] CreateSystemSettingRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateSystemSettingCommand(request.Key, request.Value, request.Description);
        var setting = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetSystemSettingByKey), new { key = setting.Key }, setting);
    }

    [HttpPatch("{key}/value")]
    public async Task<ActionResult<SystemSettingDto>> UpdateSystemSettingValue(
        string key,
        [FromBody] UpdateSystemSettingValueRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSystemSettingValueCommand(key, request.Value);
        var setting = await _mediator.Send(command, cancellationToken);

        return Ok(setting);
    }

    [HttpDelete("{key}")]
    public async Task<IActionResult> DeleteSystemSetting(string key, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteSystemSettingCommand(key), cancellationToken);
        return NoContent();
    }
}