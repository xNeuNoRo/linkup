using FluentValidation;
using LinkUpPro.Application.DTOs.Post.Requests;

namespace LinkUpPro.Application.Validators.Post;

public class CreatePostRequestValidator : AbstractValidator<CreatePostRequest>
{
    public CreatePostRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("El contenido es requerido.")
            .MaximumLength(1000)
            .WithMessage("El contenido no puede exceder 1000 caracteres.");
    }
}
