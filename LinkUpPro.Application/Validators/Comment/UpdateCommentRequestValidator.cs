using FluentValidation;
using LinkUpPro.Application.DTOs.Comment.Requests;

namespace LinkUpPro.Application.Validators.Comment;

public class UpdateCommentRequestValidator : AbstractValidator<UpdateCommentRequest>
{
    public UpdateCommentRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("El contenido es requerido.")
            .MaximumLength(500).WithMessage("El contenido no puede exceder 500 caracteres.");
    }
}
