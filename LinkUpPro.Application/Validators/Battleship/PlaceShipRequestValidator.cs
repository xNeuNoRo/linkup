using FluentValidation;
using LinkUpPro.Application.DTOs.Battleship.Requests;
using LinkUpPro.Domain.Common;

namespace LinkUpPro.Application.Validators.Battleship;

public class PlaceShipRequestValidator : AbstractValidator<PlaceShipRequest>
{
    public PlaceShipRequestValidator()
    {
        RuleFor(x => x.ShipSize)
            .Must(size => DomainConstants.RequiredBattleshipFleetSizes.Contains(size))
            .WithMessage(
                "El tamano del barco no es valido. Los tamanos permitidos son 2, 3, 4 y 5."
            );

        RuleFor(x => x.StartX)
            .InclusiveBetween(0, 11)
            .WithMessage("La coordenada X debe estar entre 0 y 11.");

        RuleFor(x => x.StartY)
            .InclusiveBetween(0, 11)
            .WithMessage("La coordenada Y debe estar entre 0 y 11.");

        RuleFor(x => x.Direction)
            .InclusiveBetween(0, 3)
            .WithMessage("La direccion debe ser un valor entre 0 y 3.");
    }
}
