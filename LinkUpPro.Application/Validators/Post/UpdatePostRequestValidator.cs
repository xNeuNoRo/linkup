using FluentValidation;
using LinkUpPro.Application.DTOs.Post.Requests;

namespace LinkUpPro.Application.Validators.Post;

public class UpdatePostRequestValidator : AbstractValidator<UpdatePostRequest>
{
    public UpdatePostRequestValidator()
    {
        When(
            x => x.Content != null,
            () =>
            {
                RuleFor(x => x.Content!)
                    .MaximumLength(1000)
                    .WithMessage("El contenido no puede exceder 1000 caracteres.");
            }
        );
    }
}
