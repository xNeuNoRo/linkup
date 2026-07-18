using FluentValidation;
using LinkUpPro.Application.DTOs.Battleship.Requests;

namespace LinkUpPro.Application.Validators.Battleship;

public class CreateGameRequestValidator : AbstractValidator<CreateGameRequest>
{
    public CreateGameRequestValidator()
    {
        RuleFor(x => x.OpponentId).NotEmpty().WithMessage("El oponente es requerido.");
    }
}
