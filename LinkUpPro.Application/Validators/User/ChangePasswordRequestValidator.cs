using FluentValidation;
using LinkUpPro.Application.DTOs.User.Requests;

namespace LinkUpPro.Application.Validators.User;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("La contrasena actual es requerida.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("La nueva contrasena es requerida.")
            .MinimumLength(8)
            .WithMessage("La nueva contrasena debe tener al menos 8 caracteres.")
            .Matches("[A-Z]")
            .WithMessage("La nueva contrasena debe contener al menos una mayuscula.")
            .Matches("[a-z]")
            .WithMessage("La nueva contrasena debe contener al menos una minuscula.")
            .Matches("[0-9]")
            .WithMessage("La nueva contrasena debe contener al menos un numero.")
            .Matches("[^a-zA-Z0-9]")
            .WithMessage("La nueva contrasena debe contener al menos un caracter especial.")
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("La nueva contrasena debe ser diferente de la contrasena actual.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithMessage("La confirmacion de contrasena es requerida.")
            .Equal(x => x.NewPassword)
            .WithMessage("Las contrasenas no coinciden.");
    }
}
