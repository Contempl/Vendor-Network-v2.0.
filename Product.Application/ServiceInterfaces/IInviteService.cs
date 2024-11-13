using Product.Domain.Entity;
using Product.Infrastructure.Dto;

namespace Product.Application.ServiceInterfaces;

public interface IInviteService
{
	Task CreateAsync(Invite invite);
	Task<Invite> GetByIdAsync(int inviteId);
	Task<Invite> GetInviteWithUserAsync(int inviteId);
	void ValidateInvite(Invite invite);
	Task UpdateAsync(Invite invite);
	Task DeleteAsync(Invite invite);
	void UpdateInvitationStatusAsync(User user, Invite invite, UserRegistrationByInviteDto dto);
	Invite CreateInvite(User user, Administrator admin);

}
