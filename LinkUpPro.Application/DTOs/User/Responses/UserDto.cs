namespace LinkUpPro.Application.DTOs.User.Responses;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfilePicturePath { get; set; }
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool IsVerified => EmailConfirmed;
    public string? Role { get; set; }
}
