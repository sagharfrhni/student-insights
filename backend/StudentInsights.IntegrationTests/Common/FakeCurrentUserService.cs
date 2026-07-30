using StudentInsights.Application.Common.Interfaces;

namespace StudentInsights.IntegrationTests.Common;

/// <summary>
/// Trivial ICurrentUserService for handler tests — a settable UserId
/// instead of mocking-framework ceremony for a one-property interface.
/// Throws if read before being set, so a test that forgets to set it
/// gets a clear signal instead of a confusing Guid.Empty-driven failure
/// deep inside a handler.
/// </summary>
public sealed class FakeCurrentUserService : ICurrentUserService
{
    private Guid? _userId;

    public Guid UserId
    {
        get => _userId ?? throw new InvalidOperationException(
            "FakeCurrentUserService.UserId was read before being set.");
        set => _userId = value;
    }
}