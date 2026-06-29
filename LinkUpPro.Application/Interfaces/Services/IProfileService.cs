using LinkUpPro.Application.DTOs.Profile.Requests;
using LinkUpPro.Application.DTOs.Profile.Responses;
using LinkUpPro.Domain.Common;

namespace LinkUpPro.Application.Interfaces.Services;

public interface IProfileService
{
    Task<Result<UserProfileResponseDto>> GetProfileAsync(string userId);

    Task<Result<EditProfileResponseDto>> UpdateProfileAsync(
        string userId,
        UpdateProfileRequest request
    );

    Task<Result<EditProfileResponseDto>> ChangePasswordAsync(
        string userId,
        ChangePasswordRequest request
    );

    Task<UserResponseDto?> GetByIdAsync(string id);

    Task<UserResponseDto?> GetByUserNameAsync(string userName);

    Task<UserResponseDto?> GetByEmailAsync(string email);

    Task<IReadOnlyDictionary<string, UserResponseDto>> GetByIdsAsync(
        IEnumerable<string> ids,
        CancellationToken cancellationToken = default
    );
}
