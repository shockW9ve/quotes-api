using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Quotes.Api.Data;

namespace Quotes.Api.Tests.Infrastructure;

public class QuotesApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;
    public QuotesApiFactory(string connectionString) => _connectionString = connectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(cfg =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = _connectionString
            }!);
        });

        builder.ConfigureServices(services =>
        {
            // optional: ensure DB is created/migrated
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<QuotesDbContext>();
            db.Database.Migrate();
        });
    }
}

