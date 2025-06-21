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

	public async Task CreateAsync(Invite invite)
	{
		await _invites.AddAsync(invite);
		await SaveAsync();
	}
	public async Task DeleteAsync(Invite invite)
	{
		_invites.Remove(invite);
		await SaveAsync();
	}
	public IQueryable<Invite> GetAll() => _invites;
	public async Task<Invite?> GetByIdOrDefaultAsync(int inviteId) => await _invites.SingleOrDefaultAsync(w => w.Id == inviteId);
	public async Task<Invite> GetByIdAsync(int inviteId) => await _invites.SingleAsync(w => w.Id == inviteId);
	public async Task UpdateAsync(Invite invite)
	{
		_invites.Update(invite);
		await SaveAsync();
	}
	public async Task<Invite> GetInviteWithUserAsync(int inviteId)
	{
		var invite = await GetAll()
			.Where(i => i.Id == inviteId)
			.Include(i => i.InvitedUser)
			.FirstAsync();

		return invite;
	}

	private async Task SaveAsync() => await _context.SaveChangesAsync();
}
