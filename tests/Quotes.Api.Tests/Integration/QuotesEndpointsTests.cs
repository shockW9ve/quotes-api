using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Quotes.Api.Contracts;
using Quotes.Api.Tests.Infrastructure;
using Xunit.Abstractions;

namespace Quotes.Api.Tests.Integration;

public class QuotesEndpointsTests : IClassFixture<PostgresContainerFixture>
{
    private readonly PostgresContainerFixture _fx;

    public QuotesEndpointsTests(PostgresContainerFixture fx) => _fx = fx;

    [Fact]
    public async Task Post_Then_Get_Returns_Created_Item()
    {
        await using var factory = new QuotesApiFactory(_fx.ConnectionString);
        using var client = factory.CreateClient();

        var req = new CreateQuoteRequest("Stay hungry", "Steve");
        var post = await client.PostAsJsonAsync("/quotes", req);
        post.StatusCode.Should().Be(HttpStatusCode.Created);

        var get = await client.GetAsync("/quotes");
        get.StatusCode.Should().Be(HttpStatusCode.OK);

        var items = await get.Content.ReadFromJsonAsync<List<QuoteResponse>>();
        items.Should().NotBeNull();
        items!.Should().ContainSingle(i => i.Text == "Stay hungry" && i.Author == "Steve");
    }
}

