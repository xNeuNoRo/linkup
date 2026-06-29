using FluentValidation;
using LinkUpPro.Application.DTOs.Battleship.Requests;
using LinkUpPro.Domain.Common;

namespace LinkUpPro.Application.Validators.Battleship;

public class PlaceShipRequestValidator : AbstractValidator<PlaceShipRequest>
{
    public PlaceShipRequestValidator()
    {
        RuleFor(x => x.StartX)
            .InclusiveBetween(0, DomainConstants.BoardSize - 1)
            .WithMessage($"La coordenada X debe estar entre 0 y {DomainConstants.BoardSize - 1}.");

        RuleFor(x => x.StartY)
            .InclusiveBetween(0, DomainConstants.BoardSize - 1)
            .WithMessage($"La coordenada Y debe estar entre 0 y {DomainConstants.BoardSize - 1}.");

        RuleFor(x => x.Direction)
            .InclusiveBetween(1, 4)
            .WithMessage(
                "La dirección seleccionada no es válida. Las opciones válidas son: 1 (Arriba), 2 (Abajo), 3 (Derecha), 4 (Izquierda)."
            );

        RuleFor(x => x.ShipSize)
            .Must(size => DomainConstants.RequiredBattleshipFleetSizes.Contains(size))
            .WithMessage(
                "El tamaño del barco no es válido. Los tamaños permitidos son 2, 3, 4 y 5."
            );
    }
}
