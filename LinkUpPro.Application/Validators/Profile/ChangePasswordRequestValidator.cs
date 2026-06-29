using FluentValidation;
using LinkUpPro.Application.DTOs.Profile.Requests;

namespace LinkUpPro.Application.Validators.Profile;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("La contraseña actual es requerida.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("La nueva contraseña es requerida.")
            .MinimumLength(8)
            .WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .Matches("[A-Z]")
            .WithMessage("La contraseña debe contener al menos una mayúscula.")
            .Matches("[a-z]")
            .WithMessage("La contraseña debe contener al menos una minúscula.")
            .Matches("[0-9]")
            .WithMessage("La contraseña debe contener al menos un número.")
            .Matches("[^a-zA-Z0-9]")
            .WithMessage("La contraseña debe contener al menos un carácter especial.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithMessage("La confirmación de contraseña es requerida.")
            .Equal(x => x.NewPassword)
            .WithMessage("La nueva contraseña y su confirmación no coinciden.");

        When(
            x => !string.IsNullOrEmpty(x.NewPassword) && !string.IsNullOrEmpty(x.CurrentPassword),
            () =>
            {
                RuleFor(x => x.NewPassword)
                    .NotEqual(x => x.CurrentPassword)
                    .WithMessage("La nueva contraseña debe ser diferente de la contraseña actual.");
            }
        );
    }
}
