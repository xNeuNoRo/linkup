namespace LinkUpPro.Application.ViewModels.ProfileViewModels;

/// <summary>
/// ViewModel principal de la pantalla "Mi Perfil" (solo lectura de algunos campos).
/// </summary>
public class ProfileViewModel
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? ProfilePicturePath { get; set; }
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastActivityAt { get; set; }

    public int TotalPosts { get; set; }
    public int ImagePosts { get; set; }
    public int VideoPosts { get; set; }
    public int FriendsOnlyPosts { get; set; }
    public int OnlyMePosts { get; set; }
    public int EditedPosts { get; set; }
}
