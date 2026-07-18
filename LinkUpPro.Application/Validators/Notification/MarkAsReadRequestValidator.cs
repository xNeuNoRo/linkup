using FluentValidation;
using LinkUpPro.Application.DTOs.Notification.Requests;

namespace LinkUpPro.Application.Validators.Notification;

public class MarkAsReadRequestValidator : AbstractValidator<MarkAsReadRequest>
{
    public MarkAsReadRequestValidator()
    {
        RuleFor(x => x.NotificationId).GreaterThan(0).WithMessage("La notificacion no es valida.");
    }
}
