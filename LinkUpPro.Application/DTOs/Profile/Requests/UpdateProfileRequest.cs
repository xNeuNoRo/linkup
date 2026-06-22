namespace LinkUpPro.Application.DTOs.Profile.Requests;

public record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string PhoneNumber,
    string? ProfilePicturePath = null
);
