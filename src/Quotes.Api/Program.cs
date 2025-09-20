using FluentValidation;
using FluentValidation.AspNetCore;
// using Quotes.Api.Contracts;
// using Quotes.Api.Validation;
// using Quotes.Api.Models;
// using Quotes.Api.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// fluentvalidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateQuoteRequestValidator>();

// in-memory repo - todo replace with ef
builder.Services.AddSingleton<IQuotesRepository, InMemoryQuotesRepository>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();

public partial class Program { }
