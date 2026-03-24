using OneOf;
using OneOf.Types;
using Product.Application.Dto;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Result;

namespace Product.Application.ServiceInterfaces;

public interface IInviteService
{
	Invite CreateInvite(User user, User sender);

	Invite CreateInviteByAdmin(User user, Administrator sender);
	Task<OneOf<InviteDtoWithStatus, Error>> GetInviteById(int inviteId, CancellationToken cancellationToken);
	Task<OneOf<UserDtoToFrontEnd, ValidationError, NotFoundError, Error>> RegisterByInvite(int inviteId, UserRegistrationByInviteDto registrationData, CancellationToken cancellationToken);
}
