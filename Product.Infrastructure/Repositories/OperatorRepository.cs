using Microsoft.EntityFrameworkCore;
using Product.Application.Interfaces;
using Product.Domain.Entity;

namespace Product.Infrastructure.Repositories;

public class OperatorRepository : IOperatorRepository
{
	private readonly AppDbContext _context;
	private readonly DbSet<Operator> _operators;

	public OperatorRepository(AppDbContext context)
	{
		_context = context;
		_operators = _context.Operators;
	}
	public IQueryable<Operator> GetAll() => _operators;
	public async Task CreateAsync(Operator @operator)
	{
		await _operators.AddAsync(@operator);
		await SaveAsync();
	}
	public async Task DeleteAsync(Operator @operator)
	{
		_operators.Remove(@operator);
		await SaveAsync();
	}
	public async Task<List<Operator>> GetAllAsync() => await _operators.ToListAsync();
	public async Task<Operator?> GetByIdOrDefaultAsync(int operatorId) => await _operators.SingleOrDefaultAsync(oper => oper.Id == operatorId);
	public async Task<Operator> GetByIdAsync(int operatorId) => await _operators.SingleAsync(oper => oper.Id == operatorId);
	public async Task UpdateAsync(Operator @operator)
	{
		_operators.Update(@operator);
		await SaveAsync();
	}
	public async Task<List<Operator>> GetOperatorsByNameAsync(string operatorName)
	{
		var operators = await _operators.Where(op => op.BusinessName.Contains(operatorName))
			.ToListAsync();

		return operators;
	}
	public async Task SaveAsync() => await _context.SaveChangesAsync();
}
