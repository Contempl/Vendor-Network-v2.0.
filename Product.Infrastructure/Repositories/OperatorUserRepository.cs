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

	public async Task CreateAsync(OperatorUser operatorUser)
	{
		await _operatorUsers.AddAsync(operatorUser);
		await SaveAsync();
	}
	public IQueryable<OperatorUser> GetAll() => _operatorUsers;
	public async Task DeleteAsync(OperatorUser operatorUser)
	{
		_operatorUsers.Remove(operatorUser);
		await SaveAsync();
	}
	public async Task<OperatorUser?> GetByIdOrDefaultAsync(int operatorId) => await _operatorUsers.SingleOrDefaultAsync(w => w.Id == operatorId);
	public async Task<OperatorUser> GetByIdAsync(int operatorId) => await _operatorUsers.SingleAsync(w => w.Id == operatorId);
	public async Task UpdateAsync(OperatorUser operatorUser)
	{
		_operatorUsers.Update(operatorUser);
		await SaveAsync();
	}
	public async Task SaveAsync() => await _context.SaveChangesAsync();
}
