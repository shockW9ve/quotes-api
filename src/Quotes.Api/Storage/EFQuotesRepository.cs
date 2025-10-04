using Microsoft.EntityFrameworkCore;
using Quotes.Api.Data;
using Quotes.Api.Models;

namespace Quotes.Api.Storage;

public class EFQuotesRepository : IQuotesRepository
{
    private readonly QuotesDbContext _db;

    public EFQuotesRepository(QuotesDbContext db) => _db = db;

    public async Task<IReadOnlyList<Quote>> GetAllAsync()
    {
        return await _db.Quotes.AsNoTracking().OrderBy(q => q.CreatedAt).ToListAsync();
    }

    public async Task<Quote> CreateAsync(string text, string author)
    {
        var quote = new Quote(Guid.NewGuid(), text.Trim(), author.Trim(), DateTimeOffset.UtcNow);
        _db.Add(quote);
        await _db.SaveChangesAsync();
        return quote;
    }
}
