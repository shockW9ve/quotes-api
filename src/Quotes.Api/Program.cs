using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Quotes.Api.Contracts;
using Quotes.Api.Data;
using Quotes.Api.Validation;
using Quotes.Api.Models;
using Quotes.Api.Storage;
using Serilog;
using System.Threading.RateLimiting;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using HealthChecks.NpgSql;


// serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

// cors
var allowedOrigins = builder.Configuration["Cors:Ui"] ?? "http://localhost:4200";
builder.Services.AddCors(options =>
        {
            options.AddPolicy("ui", policy => policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });

builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console());

// rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", o =>
    {
        o.PermitLimit = 100;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 0;
    });
});

// connection string
var connectionString = builder.Configuration.GetConnectionString("Default") ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default");

// in-memory repo
// builder.Services.AddSingleton<IQuotesRepository, InMemoryRepository>();
// ef core
// builder.Services.AddDbContext<QuotesDbContext>(options => options.UseNpgsql(connectionString));
// builder.Services.AddDbContextPool<QuotesDbContext>(opt =>
builder.Services.AddDbContext<QuotesDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(3),
            errorCodesToAdd: null);
        npgsql.CommandTimeout(10);
    })
    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

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

// problem details
builder.Services.AddProblemDetails();

builder.Services.AddScoped<IQuotesRepository, EFQuotesRepository>();
builder.Services.AddSerilog();
builder.Services.AddOpenApi();

var app = builder.Build();

// apply migrations automatically on boot (works in compose/k8s)
// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<QuotesDbContext>();
//     db.Database.Migrate();
// }

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (app.Environment.IsDevelopment() is false)
{
    app.UseHsts();
}

app.Use(async (ctx, next) =>
        {
            ctx.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
            ctx.Response.Headers.TryAdd("X-Frame-Options", "DENY");
            ctx.Response.Headers.TryAdd("Referrer-Policy", "no-referrer");
            // X-XSS-Protection obsolete in modern browsers; omit or set "0"
            await next();
        });

// rate limiter
app.UseRateLimiter();

// cors
app.UseCors("ui");

// unified error response
app.UseExceptionHandler();

// map health endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/ready", new() { Predicate = r => r.Tags.Contains("ready") });

// quotes endpoints
app.MapGet("/quotes", async (IQuotesRepository repo) =>
        {
            var items = await repo.GetAllAsync();
            return Results.Ok(items.Select(q => new QuoteResponse(q.Id, q.Text, q.Author, q.CreatedAt)));
        }).RequireRateLimiting("fixed");

app.MapPost("/quotes", async (CreateQuoteRequest req, IQuotesRepository repo, IValidator<CreateQuoteRequest> validator) =>
        {
            var result = await validator.ValidateAsync(req);
            if (result.IsValid is false)
            {
                return Results.ValidationProblem(result.ToDictionary(), statusCode: 422);
            }

            var created = await repo.CreateAsync(req.Text, req.Author);
            return Results.Created($"/quotes/{created.Id}", new QuoteResponse(created.Id, created.Text, created.Author, created.CreatedAt));
        }).RequireRateLimiting("fixed");



app.UseHttpsRedirection();

app.Run();

public partial class Program { }
