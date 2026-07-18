namespace LinkUpPro.Application.ViewModels.FriendshipViewModels;

/// <summary>
/// ViewModel para un item de amigo en listados (Amigos, Perfil de amigo, Amigos en común).
/// </summary>
public class FriendListItemViewModel
{
    public string FriendId { get; set; } = string.Empty;
    public string FriendName { get; set; } = string.Empty;
    public string FriendUserName { get; set; } = string.Empty;
    public string? FriendProfilePicturePath { get; set; }
    public int CommonFriendsCount { get; set; }
    public DateTime FriendshipCreatedAt { get; set; }
}
