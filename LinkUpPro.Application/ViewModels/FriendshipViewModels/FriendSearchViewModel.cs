using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Application.ViewModels.FriendshipViewModels;

/// <summary>
/// ViewModel para el buscador de amigos en la pantalla "Amigos".
/// </summary>
public class FriendSearchViewModel
{
    [Display(Name = "Criterio de búsqueda")]
    public string SearchText { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;
}
