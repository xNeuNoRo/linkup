using FluentValidation;
using LinkUpPro.Application.DTOs.Comment.Requests;

namespace LinkUpPro.Application.Validators.Comment;

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("El contenido es requerido.")
            .MaximumLength(500)
            .WithMessage("El contenido no puede exceder 500 caracteres.");
    }
}
