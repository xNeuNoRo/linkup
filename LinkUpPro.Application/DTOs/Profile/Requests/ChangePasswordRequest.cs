namespace LinkUpPro.Application.DTOs.Profile.Requests;

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword
);
