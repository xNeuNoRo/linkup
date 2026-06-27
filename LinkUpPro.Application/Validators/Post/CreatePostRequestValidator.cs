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

        RuleFor(x => x.ContentType)
            .InclusiveBetween(1, 2)
            .WithMessage("El tipo de contenido seleccionado no es válido.");

        RuleFor(x => x.Privacy)
            .InclusiveBetween(1, 2)
            .WithMessage("La privacidad seleccionada no es válida.");

        // Validación condicional para Image (ContentType=1)
        When(x => x.ContentType == 1, () =>
        {
            RuleFor(x => x.ImageFile)
                .NotNull()
                .WithMessage("Debe seleccionar una imagen para crear la publicación.");
        });

        // Validación condicional para YouTube (ContentType=2)
        When(x => x.ContentType == 2, () =>
        {
            RuleFor(x => x.YouTubeUrl)
                .NotEmpty()
                .WithMessage("Debe ingresar un enlace válido de YouTube.");
        });
    }
}
