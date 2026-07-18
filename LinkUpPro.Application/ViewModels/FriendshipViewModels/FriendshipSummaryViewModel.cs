namespace LinkUpPro.Application.ViewModels.FriendshipViewModels;

/// <summary>
/// ViewModel para los indicadores de resumen en la parte superior de la pantalla "Amigos".
/// Total de amigos activos y publicaciones disponibles (Solo amigos, activas y visibles).
/// </summary>
public class FriendshipSummaryViewModel
{
    public int TotalActiveFriends { get; set; }
    public int AvailablePostsCount { get; set; }
}
