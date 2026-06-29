using Microsoft.AspNetCore.Http;

namespace LinkUpPro.Application.DTOs.User.Requests;

public record RegisterRequest(
    string UserName,
    string Email,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName,
    string PhoneNumber,
    IFormFile ProfilePictureFile
);