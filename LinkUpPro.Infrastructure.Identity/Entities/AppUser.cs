using Microsoft.AspNetCore.Identity;

namespace LinkUpPro.Infrastructure.Identity.Entities;

public class AppUser : IdentityUser<string>
{
    public AppUser()
    {
        Id = Guid.NewGuid().ToString("N");
    }

    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? ProfilePicturePath { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTimeOffset? LastActivityAt { get; set; }
    public DateTime? LastActivationEmailSentAt { get; set; }

    public string GetDisplayName() => $"{FirstName} {LastName}".Trim();
}
