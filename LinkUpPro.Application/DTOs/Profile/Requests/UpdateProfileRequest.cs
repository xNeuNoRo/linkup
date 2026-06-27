using Microsoft.AspNetCore.Http;

namespace LinkUpPro.Application.DTOs.Profile.Requests;

public record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string PhoneNumber,
    IFormFile? ProfilePictureFile = null
);
