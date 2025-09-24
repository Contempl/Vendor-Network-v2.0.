using Product.Domain.Entity;

namespace Product.Application.Interfaces;

public interface IInviteRepository : IRepository<Invite>
{
	Task<Invite> GetByIdAsync(int inviteId, CancellationToken cancellationToken);
	Task<Invite> GetInviteWithUserAsync(int inviteId, CancellationToken cancellationToken);
}