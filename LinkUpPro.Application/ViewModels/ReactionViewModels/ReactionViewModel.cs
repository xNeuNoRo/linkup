using System.ComponentModel.DataAnnotations;
using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Application.ViewModels.ReactionViewModels;

/// <summary>
/// ViewModel para crear o alternar una reacción en una publicación.
/// Solo dos valores posibles: Like (1) o Dislike (2).
/// </summary>
public class ReactionViewModel
{
    [Required]
    [Display(Name = "Identificador de publicación")]
    public long PostId { get; set; }

    /// <summary>
    /// Tipo de reacción: 1 = Like, 2 = Dislike. Mapea a ReactionType.
    /// </summary>
    [Required(ErrorMessage = "Debe seleccionar un tipo de reacción.")]
    [Range(1, 2, ErrorMessage = "El tipo de reacción debe ser 'Me gusta' (1) o 'No me gusta' (2).")]
    [Display(Name = "Tipo de reacción")]
    public int Type { get; set; }

    public ReactionViewModel() { }

    public ReactionViewModel(long postId, ReactionType type)
    {
        PostId = postId;
        Type = (int)type;
    }
}
