using Microsoft.EntityFrameworkCore;
using Product.Application.Interfaces;
using Product.Domain.Entity;

namespace Product.Infrastructure.Repositories;

public class OperatorIndustryRepository : IOperatorIndustryRepository
{
	private readonly AppDbContext _context;
	private readonly DbSet<OperatorIndustry> _operatorIndustries;

	public OperatorIndustryRepository(AppDbContext context)
	{
		_context = context;
		_operatorIndustries = _context.OperatorIndustries;
	}

	public async Task CreateAsync(OperatorIndustry entity, CancellationToken cancellationToken = default)
	{
		await _operatorIndustries.AddAsync(entity, cancellationToken);
		await SaveAsync(cancellationToken);
	}
	public Task DeleteAsync(OperatorIndustry industry, CancellationToken cancellationToken = default)
	{
		_operatorIndustries.Remove(industry);
		return SaveAsync(cancellationToken);
	}
	public IQueryable<OperatorIndustry> GetAll() => _operatorIndustries;
	public Task<OperatorIndustry?> GetByIdOrDefaultAsync(int industryId) => 
		_operatorIndustries.SingleOrDefaultAsync(oper => oper.Id == industryId);
	public Task<OperatorIndustry> GetByIdAsync(int operatorId, int industryId, CancellationToken cancellationToken = default) => 
		_operatorIndustries.SingleAsync(oper => 
			oper.Id == industryId && oper.OperatorId == operatorId, cancellationToken: cancellationToken);

	public Task<List<OperatorIndustry>> GetOperatorsIndustriesAsync(int operatorId, CancellationToken cancellationToken = default) => 
		GetAll()
			.Where(i => i.OperatorId == operatorId).ToListAsync(cancellationToken: cancellationToken);

	private Task SaveAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
	public Task UpdateAsync(OperatorIndustry entity, CancellationToken cancellationToken = default)
	{
		_operatorIndustries.Update(entity);
		return SaveAsync(cancellationToken);
	}
}
