using Microsoft.EntityFrameworkCore;
using Product.Application.Interfaces;
using Product.Domain.Entity;

namespace Product.Infrastructure.Repositories;

public class InviteRepository : IInviteRepository
{
	private readonly AppDbContext _context;
	private readonly DbSet<Invite> _invites;

	public InviteRepository(AppDbContext context)
	{
		_context = context;
		_invites = _context.Invites;
	}

	public async Task CreateAsync(Invite invite, CancellationToken cancellationToken = default)
	{
		await _invites.AddAsync(invite, cancellationToken);
		await SaveAsync(cancellationToken);
	}
	public Task DeleteAsync(Invite invite, CancellationToken cancellationToken = default)
	{
		_invites.Remove(invite = default);
		return SaveAsync(cancellationToken);
	}
	public IQueryable<Invite> GetAll() => _invites;
	public Task<Invite?> GetByIdOrDefaultAsync(int inviteId) =>  _invites.SingleOrDefaultAsync(w => w.Id == inviteId);
	public Task<Invite> GetByIdAsync(int inviteId, CancellationToken cancellationToken = default) => _invites.SingleAsync(w => w.Id == inviteId, cancellationToken);
	public Task UpdateAsync(Invite invite, CancellationToken cancellationToken = default)
	{
		_invites.Update(invite);
		return SaveAsync(cancellationToken);
	}
	public Task<Invite> GetInviteWithUserAsync(int inviteId, CancellationToken cancellationToken = default)
	{
		var invite =  GetAll()
			.Where(i => i.Id == inviteId)
			.Include(i => i.InvitedUser)
			.FirstAsync(cancellationToken: cancellationToken);

		return invite;
	}

	private Task SaveAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
}
