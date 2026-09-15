using FluentValidation;
using MainServer.DTOs.Products;

namespace MainServer.Validators;

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock must be greater than or equal to 0.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category id must be greater than 0.");
    }
}
