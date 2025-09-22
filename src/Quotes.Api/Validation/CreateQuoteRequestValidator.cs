using FluentValidation;
using Quotes.Api.Contracts;

namespace Quotes.Api.Validation;

public class CreateQuoteRequestValidator : AbstractValidator<CreateQuoteRequest>
{
    public CreateQuoteRequestValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty()
            .WithMessage("Text is required")
            .MaximumLength(500);


        RuleFor(x => x.Author)
            .NotEmpty()
            .WithMessage("Author is required")
            .MaximumLength(500);
    }
}
