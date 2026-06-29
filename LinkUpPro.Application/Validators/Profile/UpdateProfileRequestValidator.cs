using FluentValidation;
using LinkUpPro.Application.DTOs.Profile.Requests;

namespace LinkUpPro.Application.Validators.Profile;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
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
    }
}
