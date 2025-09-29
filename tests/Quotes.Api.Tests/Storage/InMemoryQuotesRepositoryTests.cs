using FluentAssertions;
using Quotes.Api.Storage;

namespace Quotes.Api.Tests.Storage;

public class InMemoryQuotesRepositoryTests()
{
    [Fact]
    public async Task Create_Then_GetAll_Returns_Items()
    {
        // arrange
        string author = "author";
        string text = "some text";
        var repository = new InMemoryRepository();
        // act
        var create = await repository.CreateAsync(text, author);
        var request = await repository.GetAllAsync();
        // assert
        request.Should().ContainSingle(c => c.Id == create.Id && c.Text == create.Text && c.Author == create.Author);
    }
}
