using LinkUpPro.Application.ViewModels.PostViewModels;
using LinkUpPro.Application.ViewModels.Shared;

namespace LinkUpPro.Application.ViewModels.FriendshipViewModels;

public class FriendsFeedPartialViewModel
{
    public PostFilterViewModel Filters { get; set; } = new();
    public PagedResultViewModel<PostListItemViewModel> Posts { get; set; } = new();
    public bool HasMorePages => Posts.TotalItems > Posts.Page * Posts.PageSize;
}
