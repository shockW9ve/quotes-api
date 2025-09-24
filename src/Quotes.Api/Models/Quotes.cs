namespace Quotes.Api.Models;

public record Quote(Guid Id, string Text, string Author, DateTimeOffset CreatedAt);
