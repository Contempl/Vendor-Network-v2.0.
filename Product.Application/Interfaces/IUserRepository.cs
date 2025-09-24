using Product.Domain.Entity;

namespace Product.Application.Interfaces;

public interface IUserRepository : IRepository<User>
{
	Task<User> GetByIdAsync(int id, CancellationToken cancellationToken);
	Task<User?> GetByEmailAsync (string email, CancellationToken cancellationToken);
	Task<User> GetByIdWithInvitesAsync(int userId, CancellationToken cancellationToken);
}