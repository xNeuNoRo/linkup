using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Application.ViewModels.FriendRequestViewModels;

/// <summary>
/// ViewModel para la pantalla "Nueva solicitud de amistad".
/// Incluye buscador por nombre de usuario y tarjetas seleccionables.
/// </summary>
public class SendFriendRequestViewModel
{
    [Required(ErrorMessage = "Debe seleccionar un usuario para enviar la solicitud de amistad.")]
    [Display(Name = "Usuario seleccionado")]
    public string SelectedUserId { get; set; } = string.Empty;

    [Display(Name = "Nombre del usuario seleccionado")]
    public string SelectedUserName { get; set; } = string.Empty;

    [Display(Name = "Usuario (username) seleccionado")]
    public string SelectedUserUsername { get; set; } = string.Empty;

    [Display(Name = "Nombre de usuario (buscar)")]
    public string? SearchText { get; set; }

    public bool IsSearching { get; set; }

    public List<AvailableUserViewModel> AvailableUsers { get; set; } = [];
}
