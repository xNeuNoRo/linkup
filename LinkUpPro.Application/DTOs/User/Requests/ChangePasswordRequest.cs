namespace LinkUpPro.Application.DTOs.User.Requests;

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword
);
