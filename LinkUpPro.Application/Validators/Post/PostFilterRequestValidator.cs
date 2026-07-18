using FluentValidation;
using LinkUpPro.Application.DTOs.Post.Requests;

namespace LinkUpPro.Application.Validators.Post;

public class PostFilterRequestValidator : AbstractValidator<PostFilterRequest>
{
    public PostFilterRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("El numero de pagina debe ser mayor que cero.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("El tamano de pagina debe estar entre 1 y 100.");

        When(
            x => x.FromDate.HasValue && x.ToDate.HasValue,
            () =>
            {
                RuleFor(x => x.FromDate)
                    .LessThanOrEqualTo(x => x.ToDate)
                    .WithMessage("La fecha inicial no puede ser posterior a la fecha final.");
            }
        );
    }
}
