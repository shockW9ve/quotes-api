using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Quotes.Api.Contracts;
using Quotes.Api.Data;
using Quotes.Api.Validation;
using Quotes.Api.Models;
using Quotes.Api.Storage;
using Serilog;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using HealthChecks.NpgSql;


// serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console());

// connection string
var connectionString = builder.Configuration.GetConnectionString("Default") ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default");

// in-memory repo
// builder.Services.AddSingleton<IQuotesRepository, InMemoryRepository>();
// ef core
builder.Services.AddDbContext<QuotesDbContext>(options => options.UseNpgsql(connectionString));

// healthcheck
if (connectionString is not null)
{
    builder.Services.AddHealthChecks()
        .AddNpgSql(connectionString, name: "postgres", tags: new[] { "ready" });
}

// opentelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("quotes-api"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter());

// fluentvalidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateQuoteRequestValidator>();

builder.Services.AddScoped<IQuotesRepository, EFQuotesRepository>();
builder.Services.AddSerilog();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// map health endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/ready", new() { Predicate = r => r.Tags.Contains("ready") });

// quotes endpoints
app.MapGet("/quotes", async (IQuotesRepository repo) =>
        {
            var items = await repo.GetAllAsync();
            return Results.Ok(items.Select(q => new QuoteResponse(q.Id, q.Text, q.Author, q.CreatedAt)));
        }
        );

app.MapPost("/quotes", async (CreateQuoteRequest req, IQuotesRepository repo, IValidator<CreateQuoteRequest> validator) =>
        {
            var result = await validator.ValidateAsync(req);
            if (!result.IsValid)
            {
                return Results.ValidationProblem(result.ToDictionary());
            }

            var created = await repo.CreateAsync(req.Text, req.Author);
            return Results.Created($"/quotes/{created.Id}", new QuoteResponse(created.Id, created.Text, created.Author, created.CreatedAt));
        });


// apply migrations automatically on boot (works in compose/k8s)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<QuotesDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();

app.Run();

public partial class Program { }
