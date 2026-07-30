using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StudentInsights.Infrastructure.Persistence;

namespace StudentInsights.IntegrationTests.Common;

/// <summary>
/// Opens a fresh SQLite in-memory database and builds a real
/// <see cref="ApplicationDbContext"/> against it. SQLite in-memory (not the
/// EF Core InMemory provider) is used deliberately: this project relies on
/// real relational behavior — unique indexes, FK constraints, RowVersion
/// concurrency, global query filters — that the InMemory provider does not
/// enforce.
///
/// The underlying <see cref="SqliteConnection"/> must stay open for the
/// context's lifetime, since an in-memory SQLite database is destroyed the
/// moment its last connection closes — this class owns and disposes it.
///
/// One instance per test (never shared across tests): create it in the
/// test's constructor (or IAsyncLifetime.InitializeAsync) and dispose it in
/// Dispose()/DisposeAsync(). This gives every test a fully isolated schema
/// with zero cleanup ordering to reason about.
/// </summary>
public sealed class SqliteContextFactory : IDisposable
{
    private readonly SqliteConnection _connection;

    public ApplicationDbContext Context { get; }

    public SqliteContextFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new ApplicationDbContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}