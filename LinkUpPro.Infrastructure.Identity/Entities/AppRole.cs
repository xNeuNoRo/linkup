using Microsoft.AspNetCore.Identity;

namespace LinkUpPro.Infrastructure.Identity.Entities;

public class AppRole : IdentityRole<string>
{
    public AppRole() : base()
    {
        Id = Guid.NewGuid().ToString("N");
    }

    public AppRole(string roleName) : base(roleName)
    {
        Id = Guid.NewGuid().ToString("N");
        Name = roleName;
    }
}
