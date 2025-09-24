using Quotes.Api.Models;

namespace Quotes.Api.Storage;

public interface IQuotesRepository
{
    Task<IReadOnlyList<Quote>> GetAllAsync();
    Task<Quote> CreateAsync(string text, string author);
}
