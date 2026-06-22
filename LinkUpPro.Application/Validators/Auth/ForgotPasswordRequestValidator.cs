using FluentValidation;
using LinkUpPro.Application.DTOs.User.Requests;

namespace LinkUpPro.Application.Validators.Auth;

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().WithMessage("El nombre de usuario es requerido.");
    }
}
