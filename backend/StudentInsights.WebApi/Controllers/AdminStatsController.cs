using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentInsights.Application.Features.Admin.Stats.DTOs;
using StudentInsights.Application.Features.Admin.Stats.Queries.GetAdminStats;

namespace StudentInsights.WebApi.Controllers;

[ApiController]
[Route("api/admin/stats")]
[Authorize(Roles = "Admin")]
public class AdminStatsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminStatsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<AdminStatsDto>> GetAdminStats(CancellationToken cancellationToken)
    {
        var stats = await _mediator.Send(new GetAdminStatsQuery(), cancellationToken);
        return Ok(stats);
    }
}