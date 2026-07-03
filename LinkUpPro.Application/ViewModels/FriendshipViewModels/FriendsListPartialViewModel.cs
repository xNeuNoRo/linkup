namespace LinkUpPro.Application.ViewModels.FriendshipViewModels;

public class FriendsListPartialViewModel
{
    public FriendSearchViewModel Search { get; set; } = new();
    public List<FriendListItemViewModel> Friends { get; set; } = [];
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public bool HasMorePages => TotalCount > CurrentPage * 20;
}
