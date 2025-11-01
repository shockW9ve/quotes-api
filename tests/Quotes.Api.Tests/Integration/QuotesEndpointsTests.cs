using Microsoft.AspNetCore.Mvc;
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
        // arrange
        await using var factory = new QuotesApiFactory(_fx.ConnectionString);
        using var client = factory.CreateClient();

        // act
        var req = new CreateQuoteRequest("Stay hungry", "Steve");
        var post = await client.PostAsJsonAsync("/quotes", req);

        var get = await client.GetAsync("/quotes");

        var items = await get.Content.ReadFromJsonAsync<List<QuoteResponse>>();

        // assert
        post.StatusCode.Should().Be(HttpStatusCode.Created);
        get.StatusCode.Should().Be(HttpStatusCode.OK);
        items.Should().NotBeNull();
        items!.Should().ContainSingle(i => i.Text == "Stay hungry" && i.Author == "Steve");
    }

    [Fact]
    public async Task Post_Invalid_Request_Returns_422_ProblemDetails()
    {
        // arrange
        await using var factory = new QuotesApiFactory(_fx.ConnectionString);
        using var client = factory.CreateClient();

        // act
        var bad = new CreateQuoteRequest("", "");
        var res = await client.PostAsJsonAsync("/quotes", bad);
        var problem = await res.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        // assert
        res.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        problem!.Should().NotBeNull();
        problem.Errors.Should().ContainKey("Text");
        problem.Errors.Should().ContainKey("Author");
    }
}

