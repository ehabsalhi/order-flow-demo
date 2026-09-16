using FluentValidation;
using PaymentService.DTOs;

namespace PaymentService.Validators;

public class CreatePaymentRequestValidator : AbstractValidator<CreatePaymentRequest>
{
    public CreatePaymentRequestValidator()
    {
        RuleFor(x => x.OrderId).GreaterThan(0).WithMessage("Order id must be greater than 0.");

        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than 0.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required.")
            .Length(3)
            .WithMessage("Currency must be a 3-letter ISO code (e.g. USD).")
            .Matches("^[A-Za-z]{3}$")
            .WithMessage("Currency must contain letters only (e.g. USD).");
    }
}
