using LinkUpPro.Application.DTOs.FriendRequest.Responses;
using LinkUpPro.Application.DTOs.Profile.Responses;
using LinkUpPro.Application.DTOs.User.Responses;
using LinkUpPro.Infrastructure.Identity.Entities;
using Mapster;

namespace LinkUpPro.Infrastructure.Identity.Mappings;

public static class IdentityMappingConfig
{
    public static void RegisterMappings()
    {
        TypeAdapterConfig<AppUser, UserDto>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.UserName, src => src.UserName ?? string.Empty)
            .Map(dest => dest.Email, src => src.Email ?? string.Empty)
            .Map(dest => dest.FirstName, src => src.FirstName)
            .Map(dest => dest.LastName, src => src.LastName)
            .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
            .Map(dest => dest.ProfilePicturePath, src => src.ProfilePicturePath)
            .Map(dest => dest.IsActive, src => src.IsActive)
            .Map(dest => dest.EmailConfirmed, src => src.EmailConfirmed);

        TypeAdapterConfig<AppUser, UserProfileResponseDto>
            .NewConfig()
            .Map(dest => dest.IsVerified, src => src.EmailConfirmed)
            .Map(
                dest => dest.LastActivityAt,
                src =>
                    src.LastActivityAt.HasValue
                        ? src.LastActivityAt.Value.DateTime
                        : (DateTime?)null
            );

        TypeAdapterConfig<AppUser, AuthResponseDto>
            .NewConfig()
            .Map(dest => dest.IsVerified, src => src.EmailConfirmed);

        TypeAdapterConfig<AppUser, EditProfileResponseDto>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.FirstName, src => src.FirstName)
            .Map(dest => dest.LastName, src => src.LastName)
            .Map(dest => dest.Email, src => src.Email ?? string.Empty)
            .Map(dest => dest.UserName, src => src.UserName ?? string.Empty)
            .Map(dest => dest.IsVerified, src => src.EmailConfirmed)
            .Map(dest => dest.RequiresReLogin, src => false);

        TypeAdapterConfig<AppUser, AvailableUserDto>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => $"{src.FirstName} {src.LastName}".Trim())
            .Map(dest => dest.UserName, src => src.UserName ?? string.Empty)
            .Map(dest => dest.ProfilePicturePath, src => src.ProfilePicturePath);
    }
}
