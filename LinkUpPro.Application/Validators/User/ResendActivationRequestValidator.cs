using FluentValidation;
using LinkUpPro.Application.DTOs.User.Requests;

namespace LinkUpPro.Application.Validators.User;

public class ResendActivationRequestValidator : AbstractValidator<ResendActivationRequest>
{
    public ResendActivationRequestValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().WithMessage("El nombre de usuario es requerido.");
    }
}
