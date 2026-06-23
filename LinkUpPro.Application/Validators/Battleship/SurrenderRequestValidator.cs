using FluentValidation;
using LinkUpPro.Application.DTOs.Battleship.Requests;

namespace LinkUpPro.Application.Validators.Battleship;

public class SurrenderRequestValidator : AbstractValidator<SurrenderRequest>
{
    public SurrenderRequestValidator()
    {
        RuleFor(x => x.GameId).GreaterThan(0).WithMessage("La partida seleccionada no es valida.");
    }
}
