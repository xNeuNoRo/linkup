using LinkUpPro.Application.DTOs.Profile.Requests;
using LinkUpPro.Application.DTOs.Profile.Responses;

namespace LinkUpPro.Application.Interfaces.Services;

public interface IProfileService
{
    Task<UserProfileResponseDto> GetProfileAsync(string userId);

    Task<EditProfileResponseDto> UpdateProfileAsync(string userId, UpdateProfileRequest request);

    Task<EditProfileResponseDto> ChangePasswordAsync(string userId, ChangePasswordRequest request);

    Task<UserResponseDto?> GetByIdAsync(string id);

    Task<UserResponseDto?> GetByUserNameAsync(string userName);

    Task<UserResponseDto?> GetByEmailAsync(string email);
}
