using LinkUpPro.Domain.Enums;
using LinkUpPro.Domain.ValueObjects;
using Mapster;

namespace LinkUpPro.Application.Mappings;

public sealed record CoordinatesTuple(int X, int Y);

public static class ValueObjectMappingConfig
{
    public static void RegisterValueObjectMappings()
    {
        // ==========================================
        // PhoneNumber
        // ==========================================
        TypeAdapterConfig<PhoneNumber, string>.NewConfig()
            .MapWith(vo => vo.Value);

        TypeAdapterConfig<string, PhoneNumber>.NewConfig()
            .MapWith(s => PhoneNumber.Create(s));

        // ==========================================
        // Email
        // ==========================================
        TypeAdapterConfig<Email, string>.NewConfig()
            .MapWith(vo => vo.Value);

        TypeAdapterConfig<string, Email>.NewConfig()
            .MapWith(s => Email.Create(s));

        // ==========================================
        // YouTubeVideoId
        // ==========================================
        TypeAdapterConfig<YouTubeVideoId, string>.NewConfig()
            .MapWith(vo => vo.Value);

        TypeAdapterConfig<string, YouTubeVideoId>.NewConfig()
            .MapWith(s => YouTubeVideoId.Create(s));

        // ==========================================
        // Coordinates
        // ==========================================
        TypeAdapterConfig<Coordinates, CoordinatesTuple>.NewConfig()
            .MapWith(c => new CoordinatesTuple(c.X, c.Y));

        TypeAdapterConfig<CoordinatesTuple, Coordinates>.NewConfig()
            .MapWith(t => Coordinates.Create(t.X, t.Y));

        // ==========================================
        // ShipDirection
        // ==========================================
        TypeAdapterConfig<ShipDirection, int>.NewConfig()
            .MapWith(d => (int)d);

        TypeAdapterConfig<int, ShipDirection>.NewConfig()
            .MapWith(i => (ShipDirection)i);

        TypeAdapterConfig<ShipDirection, string>.NewConfig()
            .MapWith(d => d.ToString());

        TypeAdapterConfig<string, ShipDirection>.NewConfig()
            .MapWith(s => Enum.Parse<ShipDirection>(s, true));

        // ==========================================
        // ShipSize
        // ==========================================
        TypeAdapterConfig<ShipSize, int>.NewConfig()
            .MapWith(s => (int)s);

        TypeAdapterConfig<int, ShipSize>.NewConfig()
            .MapWith(i => (ShipSize)i);

        // ==========================================
        // PostContentType
        // ==========================================
        TypeAdapterConfig<PostContentType, int>.NewConfig()
            .MapWith(t => (int)t);

        TypeAdapterConfig<int, PostContentType>.NewConfig()
            .MapWith(i => (PostContentType)i);

        // ==========================================
        // PrivacyLevel
        // ==========================================
        TypeAdapterConfig<PrivacyLevel, int>.NewConfig()
            .MapWith(p => (int)p);

        TypeAdapterConfig<int, PrivacyLevel>.NewConfig()
            .MapWith(i => (PrivacyLevel)i);

        // ==========================================
        // ReactionType
        // ==========================================
        TypeAdapterConfig<ReactionType, int>.NewConfig()
            .MapWith(t => (int)t);

        TypeAdapterConfig<int, ReactionType>.NewConfig()
            .MapWith(i => (ReactionType)i);

        // ==========================================
        // FriendRequestStatus
        // ==========================================
        TypeAdapterConfig<FriendRequestStatus, int>.NewConfig()
            .MapWith(s => (int)s);

        TypeAdapterConfig<int, FriendRequestStatus>.NewConfig()
            .MapWith(i => (FriendRequestStatus)i);

        // ==========================================
        // GameStatus
        // ==========================================
        TypeAdapterConfig<GameStatus, int>.NewConfig()
            .MapWith(s => (int)s);

        TypeAdapterConfig<int, GameStatus>.NewConfig()
            .MapWith(i => (GameStatus)i);

        // ==========================================
        // PasswordStrength
        // ==========================================
        TypeAdapterConfig<PasswordStrength, PasswordStrength>.NewConfig();
    }
}
