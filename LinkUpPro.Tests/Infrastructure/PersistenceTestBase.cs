using LinkUpPro.Domain.Entities.Identity;
using LinkUpPro.Domain.ValueObjects;
using LinkUpPro.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Tests.Infrastructure;

/// <summary>
/// Clase base para pruebas de integración con InMemory.
/// Proporciona métodos factory para crear AppDbContext con
/// un IDateTimeProvider fijo y datos de semilla reutilizables.
/// </summary>
public abstract class PersistenceTestBase
{
    private static int _databaseCounter;

    /// <summary>
    /// Crea un nuevo AppDbContext con InMemory.
    /// El nombre de la base de datos es único por invocación.
    /// </summary>
    protected static AppDbContext CreateContext()
    {
        var dbName = $"LinkUpProTestDb_{DateTimeOffset.UtcNow.Ticks}_{++_databaseCounter}";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var context = new AppDbContext(options, new TestDateTimeProvider());
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// Crea un usuario activo de prueba con datos predeterminados.
    /// </summary>
    protected static User CreateTestUser(
        string userName = "testuser",
        string email = "test@linkuppro.com",
        string firstName = "Test",
        string lastName = "User"
    )
    {
        var result = User.Register(
            userName: userName,
            email: Email.Create(email),
            passwordHash: "AQAAAAIAAYagAAAAE...", // hash simulado
            firstName: firstName,
            lastName: lastName,
            phoneNumber: PhoneNumber.Create("809-555-1234"),
            profilePicturePath: "/images/default-avatar.png",
            createdAt: TestDateTimeProvider.FixedUtcNow
        );

        var user = result.Value;
        user.ActivateAccount(TestDateTimeProvider.FixedUtcNow);
        return user;
    }

    /// <summary>
    /// Crea y persiste un usuario activo en el contexto proporcionado.
    /// </summary>
    protected static async Task<User> SeedUserAsync(
        AppDbContext context,
        string userName = "testuser",
        string email = "test@linkuppro.com"
    )
    {
        var user = CreateTestUser(userName, email);
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }
}
