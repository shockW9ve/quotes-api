namespace Quotes.Api.Contracts;

public record QuoteResponse(Guid Id, string Text, string Author, DateTimeOffset CreatedAt);
