using FluentValidation;
using LinkUpPro.Application.DTOs.Profile.Requests;

namespace LinkUpPro.Application.Validators.Profile;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .Must(n => n?.Trim().Length > 0).WithMessage("El nombre no puede contener solo espacios.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("El apellido es requerido.")
            .Must(n => n?.Trim().Length > 0).WithMessage("El apellido no puede contener solo espacios.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("El telefono es requerido.")
            .Matches(@"^(809|829|849)-\d{3}-\d{4}$")
            .WithMessage("Debe ingresar un numero telefonico valido de Republica Dominicana.");
    }
}
