namespace LinkUpPro.Application.ViewModels.FriendRequestViewModels;

/// <summary>
/// ViewModel para un usuario disponible al que se le puede enviar solicitud de amistad.
/// Se muestra en el listado de "Nueva solicitud" con un radio button para seleccionarlo.
/// </summary>
public class AvailableUserViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? ProfilePicturePath { get; set; }
    public int CommonFriendsCount { get; set; }
}
