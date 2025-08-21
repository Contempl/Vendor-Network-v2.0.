using Product.Application.Dto;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Result;

namespace Product.Application.ServiceInterfaces;

public interface IInviteService
{
	Invite CreateInvite(User user, User sender);

	Invite CreateInviteByAdmin(User user, Administrator sender);
	Task<Response<InviteIdToFrontEnd>> Register(int inviteId);
	Task<Response<UserDtoToFrontEnd>> RegisterByInvite(int inviteId, UserRegistrationByInviteDto registrationData);
}
