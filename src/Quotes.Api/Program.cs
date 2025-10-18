using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Quotes.Api.Contracts;
using Quotes.Api.Data;
using Quotes.Api.Validation;
using Quotes.Api.Models;
using Quotes.Api.Storage;
using Serilog;

// serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// connection string
var connectionString = builder.Configuration.GetConnectionString("Default") ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default");

builder.Services.AddOpenApi();

// fluentvalidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateQuoteRequestValidator>();

// in-memory repo
// builder.Services.AddSingleton<IQuotesRepository, InMemoryRepository>();
// ef core
builder.Services.AddDbContext<QuotesDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<IQuotesRepository, EFQuotesRepository>();

// opentelemetry

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

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

app.UseHttpsRedirection();

app.Run();

public partial class Program { }
