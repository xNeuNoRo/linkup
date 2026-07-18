using Microsoft.AspNetCore.Identity;
using VO = LinkUpPro.Domain.ValueObjects;

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

    /// <summary>
    /// Establece el numero telefonico validandolo con el Value Object PhoneNumber del Domain.
    /// Lanza DomainException si el formato es invalido.
    /// </summary>
    public void SetPhoneNumber(string phoneNumber)
    {
        PhoneNumber = VO.PhoneNumber.Create(phoneNumber).Value;
    }

    /// <summary>
    /// Establece el correo electronico validandolo con el Value Object Email del Domain.
    /// Lanza DomainException si el formato es invalido.
    /// </summary>
    public void SetEmail(string email)
    {
        var validatedEmail = VO.Email.Create(email);
        Email = validatedEmail.Value;
        UserName ??= validatedEmail.Value;
        NormalizedEmail = validatedEmail.Value.ToUpperInvariant();
        NormalizedUserName = (UserName ?? validatedEmail.Value).ToUpperInvariant();
    }
}
