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
            .WithMessage("El correo electronico es requerido.")
            .EmailAddress()
            .WithMessage("Debe ingresar un correo electronico valido.");

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
            .WithMessage("La contrasena y su confirmacion no coinciden.");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("El nombre es requerido.")
            .MaximumLength(100)
            .WithMessage("El nombre no puede exceder 100 caracteres.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("El apellido es requerido.")
            .MaximumLength(100)
            .WithMessage("El apellido no puede exceder 100 caracteres.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("El telefono es requerido.")
            .Matches(@"^(809|829|849)-\d{3}-\d{4}$")
            .WithMessage("Debe ingresar un numero telefonico valido de Republica Dominicana.");
    }
}
