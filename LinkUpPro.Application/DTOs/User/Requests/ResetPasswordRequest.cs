namespace LinkUpPro.Application.DTOs.User.Requests;

public record ResetPasswordRequest(
    string Id,
    string Token,
    string Password,
    string ConfirmPassword
);
