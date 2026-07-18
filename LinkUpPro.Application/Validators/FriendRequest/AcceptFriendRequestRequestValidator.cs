using FluentValidation;
using LinkUpPro.Application.DTOs.FriendRequest.Requests;

namespace LinkUpPro.Application.Validators.FriendRequest;

public class AcceptFriendRequestRequestValidator : AbstractValidator<AcceptFriendRequestRequest>
{
    public AcceptFriendRequestRequestValidator()
    {
        RuleFor(x => x.RequestId)
            .GreaterThan(0)
            .WithMessage("La solicitud de amistad no es valida.");
    }
}
