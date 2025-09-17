using Microsoft.EntityFrameworkCore;
using Product.Application.Interfaces;
using Product.Domain.Entity;

namespace Product.Infrastructure.Repositories;

public class OperatorUserRepository : IOperatorUserRepository
{
	private readonly AppDbContext _context;
	private readonly DbSet<OperatorUser> _operatorUsers;

	public OperatorUserRepository(AppDbContext context)
	{
		_context = context;
		_operatorUsers = _context.OperatorUsers;
	}

	public async Task CreateAsync(OperatorUser operatorUser, CancellationToken cancellationToken = default)
	{
		await _operatorUsers.AddAsync(operatorUser, cancellationToken);
		await SaveAsync(cancellationToken);
	}
	public IQueryable<OperatorUser> GetAll() => _operatorUsers;
	public async Task DeleteAsync(OperatorUser operatorUser, CancellationToken cancellationToken = default)
	{
		_operatorUsers.Remove(operatorUser);
		await SaveAsync(cancellationToken);
	}
	public async Task<OperatorUser?> GetByIdOrDefaultAsync(int operatorId) => await _operatorUsers.SingleOrDefaultAsync(w => w.Id == operatorId);
	public async Task<OperatorUser> GetByIdAsync(int operatorId, CancellationToken cancellationToken = default) => 
		await _operatorUsers.SingleAsync(w => w.Id == operatorId, cancellationToken);
	public async Task UpdateAsync(OperatorUser operatorUser, CancellationToken cancellationToken = default)
	{
		var userToUpdate = await _operatorUsers.FindAsync(operatorUser.Id, cancellationToken);

		if (userToUpdate != null)
		{
			_context.Entry(userToUpdate).CurrentValues.SetValues(operatorUser);
			await SaveAsync(cancellationToken);
		}
	}

	private async Task SaveAsync(CancellationToken cancellationToken = default) => 
		await _context.SaveChangesAsync(cancellationToken);
}
