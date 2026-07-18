namespace LinkUpPro.Application.ViewModels.FriendshipViewModels;

/// <summary>
/// ViewModel para el modal/sección de "Amigos en común".
/// Se muestra al hacer click en el contador de amigos en común del listado o perfil.
/// </summary>
public class CommonFriendsViewModel
{
    public string User1Id { get; set; } = string.Empty;
    public string User2Id { get; set; } = string.Empty;
    public List<FriendListItemViewModel> Friends { get; set; } = [];
    public int TotalCount => Friends.Count;
}
