namespace LinkUpPro.Application.DTOs.User.Requests;

public record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string PhoneNumber,
    string? ProfilePicturePath = null
);
