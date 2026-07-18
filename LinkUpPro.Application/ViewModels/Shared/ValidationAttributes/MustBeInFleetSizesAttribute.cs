using System.ComponentModel.DataAnnotations;
using LinkUpPro.Domain.Common;

namespace LinkUpPro.Application.ViewModels.Shared.ValidationAttributes;

/// <summary>
/// Valida que el valor pertenezca a uno de los tamaños de barcos de la flota obligatoria:
/// 2, 3, 3, 4, 5 (tamaños únicos: { 2, 3, 4, 5 }).
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class MustBeInFleetSizesAttribute : ValidationAttribute
{
    public MustBeInFleetSizesAttribute()
        : base("El tamaño del barco no es válido. Los tamaños permitidos son 2, 3, 4 y 5.")
    {
    }

    public override bool IsValid(object? value)
    {
        if (value is null) return true;
        if (value is int size)
        {
            return DomainConstants.RequiredBattleshipFleetSizes.Contains(size);
        }
        return false;
    }
}
