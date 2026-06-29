using FluentValidation;
using LinkUpPro.Application.DTOs.Battleship.Requests;
using LinkUpPro.Domain.Common;

namespace LinkUpPro.Application.Validators.Battleship;

public class AttackRequestValidator : AbstractValidator<AttackRequest>
{
    public AttackRequestValidator()
    {
        RuleFor(x => x.X)
            .InclusiveBetween(0, DomainConstants.BoardSize - 1)
            .WithMessage($"La coordenada X debe estar entre 0 y {DomainConstants.BoardSize - 1}.");

        RuleFor(x => x.Y)
            .InclusiveBetween(0, DomainConstants.BoardSize - 1)
            .WithMessage($"La coordenada Y debe estar entre 0 y {DomainConstants.BoardSize - 1}.");
    }
}
