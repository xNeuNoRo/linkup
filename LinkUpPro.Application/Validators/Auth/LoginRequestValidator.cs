using FluentValidation;
using LinkUpPro.Application.DTOs.User.Requests;

namespace LinkUpPro.Application.Validators.Auth;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().WithMessage("El nombre de usuario es requerido.");

        RuleFor(x => x.Password).NotEmpty().WithMessage("La contraseña es requerida.");
    }
}
