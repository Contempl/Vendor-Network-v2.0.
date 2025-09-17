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
	public async Task CreateAsync(Operator @operator, CancellationToken cancellationToken = default)
	{
		await _operators.AddAsync(@operator, cancellationToken);
		await SaveAsync(cancellationToken);
	}
	public async Task DeleteAsync(Operator @operator, CancellationToken cancellationToken = default)
	{
		_operators.Remove(@operator);
		await SaveAsync(cancellationToken);
	}

	public async Task<Operator?> GetByIdOrDefaultAsync(int operatorId) => await _operators.SingleOrDefaultAsync(oper => oper.Id == operatorId);
	public async Task<Operator> GetByIdAsync(int operatorId, CancellationToken cancellationToken = default) => 
		await _operators.SingleAsync(oper => oper.Id == operatorId, cancellationToken: cancellationToken);
	public async Task UpdateAsync(Operator @operator, CancellationToken cancellationToken = default)
	{
		_operators.Update(@operator);
		await SaveAsync(cancellationToken);
	}
	public async Task<List<Operator>> GetOperatorsByNameAsync(string operatorName, 
		CancellationToken cancellationToken = default)
	{
		var operators = await _operators
			.Where(op => op.BusinessName.Contains(operatorName))
			.Include(op => op.Industries)
			.ToListAsync(cancellationToken);

		return operators;
	}

	private async Task SaveAsync(CancellationToken cancellationToken = default) => await _context.SaveChangesAsync(cancellationToken);
}
