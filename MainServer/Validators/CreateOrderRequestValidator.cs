using FluentValidation;
using MainServer.DTOs.Orders;

namespace MainServer.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("At least one order item is required.");

        RuleForEach(x => x.Items).SetValidator(new CreateOrderItemRequestValidator());
    }
}

public class CreateOrderItemRequestValidator : AbstractValidator<CreateOrderItemRequest>
{
    public CreateOrderItemRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Product id must be greater than 0.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
    }
}
