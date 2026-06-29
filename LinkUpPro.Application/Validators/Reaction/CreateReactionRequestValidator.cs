using FluentValidation;
using LinkUpPro.Application.DTOs.Reaction.Requests;

namespace LinkUpPro.Application.Validators.Reaction;

public class CreateReactionRequestValidator : AbstractValidator<CreateReactionRequest>
{
    public CreateReactionRequestValidator()
    {
        RuleFor(x => x.PostId).GreaterThan(0).WithMessage("La publicacion no es valida.");

        RuleFor(x => x.Type)
            .InclusiveBetween(1, 2)
            .WithMessage("El tipo de reaccion debe ser 'Me gusta' (1) o 'No me gusta' (2).");
    }
}
