using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Identity;
using LinkUpPro.Domain.ValueObjects;
using LinkUpPro.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LinkUpPro.Infrastructure.Persistence.Persistence;

/// <summary>
/// Seed de datos iniciales para la aplicación, como usuarios predeterminados,
/// roles, etc. Se ejecuta automáticamente al iniciar la aplicación si la base de datos está vacía.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(
        AppDbContext context,
        IConfiguration configuration,
        IDateTimeProvider dateTimeProvider,
        CancellationToken cancellationToken = default
    )
    {
        if (await context.Users.AnyAsync(cancellationToken))
            return;

        var now = dateTimeProvider.UtcNow;

        // Usuario Administrador
        var adminEmail = Email.Create(
            configuration["SeedData:AdminEmail"] ?? "admin@linkuppro.com"
        );

        var adminPhone = PhoneNumber.Create(configuration["SeedData:AdminPhone"] ?? "809-555-0001");

        var adminResult = User.Register(
            userName: configuration["SeedData:AdminUserName"] ?? "admin",
            email: adminEmail,
            passwordHash: configuration["SeedData:AdminPasswordHash"] ?? string.Empty,
            firstName: configuration["SeedData:AdminFirstName"] ?? "Admin",
            lastName: configuration["SeedData:AdminLastName"] ?? "LinkUp",
            phoneNumber: adminPhone,
            profilePicturePath: configuration["SeedData:AdminPicture"]
                ?? "/images/default-avatar.png",
            createdAt: now
        );

        if (adminResult.IsSuccess)
        {
            var admin = adminResult.Value;
            admin.ActivateAccount(now);
            context.Users.Add(admin);
        }

        // Usuario Jugador 1 (Estándar)
        var playerEmail = Email.Create(
            configuration["SeedData:Player1Email"] ?? "player1@linkuppro.com"
        );

        var playerPhone = PhoneNumber.Create(
            configuration["SeedData:Player1Phone"] ?? "809-555-0002"
        );

        var playerResult = User.Register(
            userName: configuration["SeedData:Player1UserName"] ?? "player1",
            email: playerEmail,
            passwordHash: configuration["SeedData:Player1PasswordHash"] ?? string.Empty,
            firstName: configuration["SeedData:Player1FirstName"] ?? "Player",
            lastName: configuration["SeedData:Player1LastName"] ?? "One",
            phoneNumber: playerPhone,
            profilePicturePath: configuration["SeedData:Player1Picture"]
                ?? "/images/default-avatar.png",
            createdAt: now
        );

        if (playerResult.IsSuccess)
        {
            var player = playerResult.Value;
            player.ActivateAccount(now);
            context.Users.Add(player);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
