using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Application.Common.Interfaces;
using StudentInsights.Application.Common.Security;
using StudentInsights.Domain.Common;

namespace StudentInsights.Application.Features.Auth.Commands.ConfirmEmail;

public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand>
{
    private readonly IApplicationDbContext _context;

    public ConfirmEmailCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty)
            throw new DomainException("Invalid confirmation request.");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
            throw new DomainException("Invalid confirmation request.");

        var tokenHash = SecureTokenGenerator.Hash(request.Token);
        user.ConfirmEmail(tokenHash);

        await _context.SaveChangesAsync(cancellationToken);
    }
}