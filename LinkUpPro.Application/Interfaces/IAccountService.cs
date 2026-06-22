using LinkUpPro.Application.DTOs.User;
using LinkUpPro.Application.DTOs.User.Requests;
using LinkUpPro.Application.DTOs.User.Responses;

namespace LinkUpPro.Application.Interfaces;

public interface IAccountService
{
    Task<AuthResponseDto> LoginAsync(LoginRequest request, bool rememberMe);
    Task SignOutAsync();
    Task<AuthResponseDto> RegisterAsync(RegisterRequest request, string origin);
    Task<AuthResponseDto> ConfirmAccountAsync(string userId, string token);
    Task<AuthResponseDto> ResendActivationAsync(ResendActivationRequest request);
    Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordRequest request);
    Task<UserProfileResponseDto> GetProfileAsync(string userId);
    Task<EditProfileResponseDto> UpdateProfileAsync(string userId, UpdateProfileRequest request);
    Task<EditProfileResponseDto> ChangePasswordAsync(string userId, ChangePasswordRequest request);
    Task<UserDto?> GetByIdAsync(string id);
    Task<UserDto?> GetByUserNameAsync(string userName);
    Task<UserDto?> GetByEmailAsync(string email);
}
