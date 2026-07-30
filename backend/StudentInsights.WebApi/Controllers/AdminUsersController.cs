using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentInsights.Application.Common.Models;
using StudentInsights.Application.Features.Admin.Users.Commands.ActivateUser;
using StudentInsights.Application.Features.Admin.Users.Commands.ChangeUserRole;
using StudentInsights.Application.Features.Admin.Users.Commands.DeactivateUser;
using StudentInsights.Application.Features.Admin.Users.DTOs;
using StudentInsights.Application.Features.Admin.Users.Queries.GetUserById;
using StudentInsights.Application.Features.Admin.Users.Queries.GetUsers;
using StudentInsights.Domain.Enums;

namespace StudentInsights.WebApi.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminUsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<AdminUserListItemDto>>> GetUsers(
        [FromQuery] string? search,
        [FromQuery] UserRole? role,
        [FromQuery] bool? isActive,
        [FromQuery] PaginationParams pagination,
        CancellationToken cancellationToken)
    {
        var query = new GetUsersQuery(search, role, isActive, pagination);
        var users = await _mediator.Send(query, cancellationToken);

        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminUserDetailDto>> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        return Ok(user);
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> ActivateUser(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ActivateUserCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> DeactivateUser(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeactivateUserCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/role")]
    public async Task<ActionResult<AdminUserDetailDto>> ChangeUserRole(
        Guid id,
        [FromBody] ChangeUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeUserRoleCommand(id, request.NewRole);
        var user = await _mediator.Send(command, cancellationToken);

        return Ok(user);
    }
}