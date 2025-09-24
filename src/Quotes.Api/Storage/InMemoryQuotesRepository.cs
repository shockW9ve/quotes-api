using Quotes.Api.Models;

namespace Quotes.Api.Storage;

public class InMemoryRepository : IQuotesRepository
{
    private readonly List<Quote> _items = new List<Quote>();

    public Task<IReadOnlyList<Quote>> GetAllAsync()
        => Task.FromResult((IReadOnlyList<Quote>)_items);

    public Task<Quote> CreateAsync(string text, string author)
    {
        var q = new Quote(Guid.NewGuid(), text.Trim(), author.Trim(), DateTimeOffset.UtcNow);
        _items.Add(q);
        return Task.FromResult(q);
    }
}
