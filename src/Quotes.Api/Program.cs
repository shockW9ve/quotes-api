using FluentValidation;
using FluentValidation.AspNetCore;
using Quotes.Api.Contracts;
using Quotes.Api.Validation;
using Quotes.Api.Models;
using Quotes.Api.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// fluentvalidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateQuoteRequestValidator>();

// in-memory repo - todo replace with ef
builder.Services.AddSingleton<IQuotesRepository, InMemoryRepository>();

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
