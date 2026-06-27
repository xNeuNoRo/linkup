using FluentValidation;
using LinkUpPro.Application.DTOs.User.Requests;

namespace LinkUpPro.Application.Validators.Auth;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .WithMessage("El nombre de usuario es requerido.")
            .MaximumLength(100)
            .WithMessage("El nombre de usuario no puede exceder 100 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("El correo electrónico es requerido.")
            .EmailAddress()
            .WithMessage("Formato de correo electrónico inválido.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("La contraseña es requerida.")
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
            .Equal(x => x.Password)
            .WithMessage("La contraseña y su confirmación no coinciden.");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("El nombre es requerido.")
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("El nombre no puede ser solo espacios.")
            .MaximumLength(100)
            .WithMessage("El nombre no puede exceder 100 caracteres.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("El apellido es requerido.")
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("El apellido no puede ser solo espacios.")
            .MaximumLength(100)
            .WithMessage("El apellido no puede exceder 100 caracteres.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("El teléfono es requerido.")
            .Matches(@"^(809|829|849)-\d{3}-\d{4}$")
            .WithMessage(
                "Debe ingresar un número telefónico válido de República Dominicana (ej. 809-555-1234)."
            );

        RuleFor(x => x.ProfilePicturePath)
            .NotEmpty()
            .WithMessage("La foto de perfil es requerida.");
    }
}
