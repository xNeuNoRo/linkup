using FluentValidation;
using LinkUpPro.Application.DTOs.FriendRequest.Requests;

namespace LinkUpPro.Application.Validators.FriendRequest;

public class SendFriendRequestRequestValidator : AbstractValidator<SendFriendRequestRequest>
{
    public SendFriendRequestRequestValidator()
    {
        RuleFor(x => x.ReceiverId)
            .NotEmpty().WithMessage("El usuario receptor es requerido.");
    }
}
