using Testcontainers.PostgreSql;

namespace Quotes.Api.Tests.Infrastructure;

public sealed class PostgresContainerFixture : IAsyncLifetime
{
    public PostgreSqlContainer Container { get; private set; } = default!;
    public string ConnectionString => Container.GetConnectionString();

    public async Task InitializeAsync()
    {
        Container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithDatabase("quotes_test")
            .Build();

        await Container.StartAsync();
    }

    public Task DisposeAsync() => Container.DisposeAsync().AsTask();
}
