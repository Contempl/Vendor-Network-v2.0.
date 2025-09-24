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
	public async Task CreateAsync(Operator @operator, CancellationToken cancellationToken)
	{
		await _operators.AddAsync(@operator, cancellationToken);
		await SaveAsync(cancellationToken);
	}
	public Task DeleteAsync(Operator @operator, CancellationToken cancellationToken)
	{
		_operators.Remove(@operator);
		return SaveAsync(cancellationToken);
	}

	public Task<Operator?> GetByIdOrDefaultAsync(int operatorId) => _operators.SingleOrDefaultAsync(oper => oper.Id == operatorId);
	public Task<Operator> GetByIdAsync(int operatorId, CancellationToken cancellationToken) => _operators.SingleAsync(oper => oper.Id == operatorId, cancellationToken: cancellationToken);
	public Task UpdateAsync(Operator @operator, CancellationToken cancellationToken)
	{
		_operators.Update(@operator);
		return SaveAsync(cancellationToken);
	}
	public Task<List<Operator>> GetOperatorsByNameAsync(string operatorName, CancellationToken cancellationToken)
	{
		var operators = _operators
			.Where(op => op.BusinessName.Contains(operatorName))
			.Include(op => op.Industries)
			.ToListAsync(cancellationToken);

		return operators;
	}

	private Task SaveAsync(CancellationToken cancellationToken) => _context.SaveChangesAsync(cancellationToken);
}
