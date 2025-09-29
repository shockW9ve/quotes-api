using FluentAssertions;
using Quotes.Api.Contracts;
using Quotes.Api.Validation;

namespace Quotes.Api.Tests.Validation;

public class CreateQuoteRequestValidatorTests
{
    [Fact]
    public void Valid_Request_Passes()
    {
        // arrange
        string author = "McJackiePotato";
        string text = "Peanut butter jelly time";
        var validator = new CreateQuoteRequestValidator();
        // act
        var result = validator.Validate(new CreateQuoteRequest(text, author));
        // assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Author", "Text is required")]
    [InlineData("Some text", "", "Author is required")]
    public void Invalid_Request_Fails(string text, string author, string expectedMessage)
    {

        // arrange
        var validator = new CreateQuoteRequestValidator();
        // act
        var result = validator.Validate(new CreateQuoteRequest(text, author));
        // assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains(expectedMessage));
    }
}
