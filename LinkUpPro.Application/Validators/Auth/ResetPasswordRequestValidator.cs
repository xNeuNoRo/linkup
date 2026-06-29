using FluentValidation;
using LinkUpPro.Application.DTOs.User.Requests;

namespace LinkUpPro.Application.Validators.Auth;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El identificador de usuario es requerido.");

        RuleFor(x => x.Token).NotEmpty().WithMessage("El token es requerido.");

        RuleFor(x => x.Password).NotEmpty().WithMessage("La contrasena es requerida.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithMessage("La confirmacion de contrasena es requerida.")
            .Equal(x => x.Password)
            .WithMessage("Las contrasenas no coinciden.");
    }
}
