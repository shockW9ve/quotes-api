namespace Quotes.Api.Models;

public record Quote(Guid id, string Text, string Author, DateTimeOffset CreatedAt);
