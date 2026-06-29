using System.ComponentModel.DataAnnotations;
using LinkUpPro.Domain.Common;

namespace LinkUpPro.Application.ViewModels.BattleshipViewModels;

/// <summary>
/// ViewModel para el POST de un ataque (envío de coordenadas).
/// El tablero se actualiza con la respuesta (AttackResultViewModel).
/// </summary>
public class AttackRequestViewModel
{
    [Required]
    [Display(Name = "Identificador de partida")]
    public long GameId { get; set; }

    [Required]
    [Range(0, DomainConstants.BoardSize - 1, ErrorMessage = "La coordenada X debe estar entre 0 y 11.")]
    [Display(Name = "Coordenada X")]
    public int X { get; set; }

    [Required]
    [Range(0, DomainConstants.BoardSize - 1, ErrorMessage = "La coordenada Y debe estar entre 0 y 11.")]
    [Display(Name = "Coordenada Y")]
    public int Y { get; set; }
}
