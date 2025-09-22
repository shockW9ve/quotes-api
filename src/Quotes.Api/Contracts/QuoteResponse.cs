namespace Quotes.Api.Contracts;

public record QuoteResponse(Guid id, string Text, string Author, DateTimeOffset CreatedAt);
