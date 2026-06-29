using LinkUpPro.Application.Interfaces;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Interfaces;
using LinkUpPro.Infrastructure.Identity.Contexts;
using LinkUpPro.Infrastructure.Identity.Entities;
using LinkUpPro.Infrastructure.Identity.Mappings;
using LinkUpPro.Infrastructure.Identity.Seeds;
using LinkUpPro.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LinkUpPro.Infrastructure.Identity;

public static class ServicesRegistration
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        IdentityMappingConfig.RegisterMappings();

        var connectionString = configuration.GetConnectionString("LinkUpDb");

        services.AddDbContext<IdentityContext>(options =>
            options.UseSqlServer(
                connectionString,
                sqlOptions =>
                    sqlOptions.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName)
            )
        );

        services.Configure<IdentityOptions>(opt =>
        {
            // Password Criteria
            opt.Password.RequiredLength = 8;
            opt.Password.RequireDigit = true;
            opt.Password.RequireLowercase = true;
            opt.Password.RequireUppercase = true;
            opt.Password.RequireNonAlphanumeric = true;

            // Lockout Criteria
            opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            opt.Lockout.MaxFailedAccessAttempts = 5;
            opt.Lockout.AllowedForNewUsers = true;

            // User Criteria
            opt.User.RequireUniqueEmail = true;
            opt.SignIn.RequireConfirmedEmail = true;
        });

        services
            .AddIdentityCore<AppUser>()
            .AddRoles<IdentityRole<string>>()
            .AddSignInManager()
            .AddEntityFrameworkStores<IdentityContext>()
            .AddDefaultTokenProviders();

        // Configuramos el tiempo de vida del token para la confirmación de email
        services.Configure<DataProtectionTokenProviderOptions>(opt =>
        {
            opt.TokenLifespan = TimeSpan.FromHours(24);
        });

        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
            })
            .AddCookie(
                IdentityConstants.ApplicationScheme,
                opt =>
                {
                    // Configuramos el tiempo de vida de la cookie de autenticación
                    opt.ExpireTimeSpan = DomainConstants.PersistentSessionDuration;
                    opt.SlidingExpiration = true;
                    opt.LoginPath = "/Login";
                    opt.AccessDeniedPath = "/Login/AccessDenied";
                    opt.LogoutPath = "/Login/Logout";
                    opt.Cookie.HttpOnly = true;
                    opt.Cookie.SameSite = SameSiteMode.Lax;
                    opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                }
            );

        services.AddAuthorization();

        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IUserDirectory, UserDirectory>();

        return services;
    }

    public static async Task RunIdentitySeedAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole<string>>>();
        var userManager = provider.GetRequiredService<UserManager<AppUser>>();
        var configuration = provider.GetRequiredService<IConfiguration>();

        // Seedeamos usuarios defaults y roles
        await DefaultRoles.SeedAsync(roleManager);
        await DefaultAdminUser.SeedAsync(userManager, configuration);
        await DefaultPlayerUser.SeedAsync(userManager, configuration);
    }
}
