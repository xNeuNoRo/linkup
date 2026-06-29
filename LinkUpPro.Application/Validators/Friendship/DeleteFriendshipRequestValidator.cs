using FluentValidation;
using LinkUpPro.Application.DTOs.Friendship.Requests;

namespace LinkUpPro.Application.Validators.Friendship;

public class DeleteFriendshipRequestValidator : AbstractValidator<DeleteFriendshipRequest>
{
    public DeleteFriendshipRequestValidator()
    {
        RuleFor(x => x.FriendId).NotEmpty().WithMessage("El amigo a eliminar es requerido.");
    }
}
