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
                    .NotEmpty()
                    .WithMessage("El contenido es requerido.")
                    .MaximumLength(1000)
                    .WithMessage("El contenido no puede exceder 1000 caracteres.");
            }
        );

        When(
            x => x.ContentType.HasValue,
            () =>
            {
                RuleFor(x => x.ContentType!.Value)
                    .InclusiveBetween(1, 2)
                    .WithMessage("El tipo de contenido seleccionado no es válido.");
            }
        );

        When(
            x => x.Privacy.HasValue,
            () =>
            {
                RuleFor(x => x.Privacy!.Value)
                    .InclusiveBetween(1, 2)
                    .WithMessage("La privacidad seleccionada no es válida.");
            }
        );

        // Validación condicional para Image (ContentType=1)
        When(x => x.ContentType == 1 && x.ImageFile is not null, () =>
        {
            RuleFor(x => x.ImageFile)
                .NotNull()
                .WithMessage("Debe seleccionar una imagen válida.");
        });

        // Validación condicional para YouTube (ContentType=2)
        When(x => x.ContentType == 2 && x.YouTubeUrl is not null, () =>
        {
            RuleFor(x => x.YouTubeUrl)
                .NotEmpty()
                .WithMessage("Debe ingresar un enlace válido de YouTube.");
        });
    }
}
