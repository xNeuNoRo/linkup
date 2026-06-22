using FluentValidation;
using LinkUpPro.Application.DTOs.User.Requests;

namespace LinkUpPro.Application.Validators.Auth;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El identificador de usuario es requerido.");

        RuleFor(x => x.Token).NotEmpty().WithMessage("El token es requerido.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("La contrasena es requerida.")
            .MinimumLength(8)
            .WithMessage("La contrasena debe tener al menos 8 caracteres.")
            .Matches("[A-Z]")
            .WithMessage("La contrasena debe contener al menos una mayuscula.")
            .Matches("[a-z]")
            .WithMessage("La contrasena debe contener al menos una minuscula.")
            .Matches("[0-9]")
            .WithMessage("La contrasena debe contener al menos un numero.")
            .Matches("[^a-zA-Z0-9]")
            .WithMessage("La contrasena debe contener al menos un caracter especial.");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password)
            .WithMessage("Las contrasenas no coinciden.");
    }
}
