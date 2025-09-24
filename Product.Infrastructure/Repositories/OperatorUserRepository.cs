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

	public async Task CreateAsync(OperatorUser operatorUser, CancellationToken cancellationToken)
	{
		await _operatorUsers.AddAsync(operatorUser, cancellationToken);
		await SaveAsync(cancellationToken);
	}
	public IQueryable<OperatorUser> GetAll() => _operatorUsers;
	public Task DeleteAsync(OperatorUser operatorUser, CancellationToken cancellationToken = default)
	{
		_operatorUsers.Remove(operatorUser);
		return SaveAsync(cancellationToken);
	}
	public Task<OperatorUser?> GetByIdOrDefaultAsync(int operatorId) => _operatorUsers.SingleOrDefaultAsync(w => w.Id == operatorId);
	public Task<OperatorUser> GetByIdAsync(int operatorId, CancellationToken cancellationToken = default) => _operatorUsers.SingleAsync(w => w.Id == operatorId);
	public Task UpdateAsync(OperatorUser operatorUser, CancellationToken cancellationToken = default)
	{
		var userToUpdate = _operatorUsers.FindAsync(operatorUser.Id, cancellationToken).Result!;

		_context.Entry(userToUpdate).CurrentValues.SetValues(operatorUser);
		return SaveAsync(cancellationToken);
	}

	private Task SaveAsync(CancellationToken cancellationToken) => _context.SaveChangesAsync(cancellationToken);
}
