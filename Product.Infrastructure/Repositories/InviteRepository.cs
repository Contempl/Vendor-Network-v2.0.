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
	public async Task DeleteAsync(Invite invite, CancellationToken cancellationToken = default)
	{
		_invites.Remove(invite);
		await SaveAsync(cancellationToken);
	}
	public IQueryable<Invite> GetAll() => _invites;
	public async Task<Invite?> GetByIdOrDefaultAsync(int inviteId) => await _invites.SingleOrDefaultAsync(w => w.Id == inviteId);
	public async Task<Invite> GetByIdAsync(int inviteId, CancellationToken cancellationToken = default) => await _invites.SingleAsync(w => w.Id == inviteId, cancellationToken);
	public async Task UpdateAsync(Invite invite, CancellationToken cancellationToken = default)
	{
		_invites.Update(invite);
		await SaveAsync(cancellationToken);
	}
	public async Task<Invite> GetInviteWithUserAsync(int inviteId, CancellationToken cancellationToken = default)
	{
		var invite = await GetAll()
			.Where(i => i.Id == inviteId)
			.Include(i => i.InvitedUser)
			.FirstAsync(cancellationToken: cancellationToken);

		return invite;
	}

	private async Task SaveAsync(CancellationToken cancellationToken = default) => await _context.SaveChangesAsync(cancellationToken);
}
