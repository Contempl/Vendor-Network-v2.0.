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

	public async Task CreateAsync(OperatorIndustry entity, CancellationToken cancellationToken)
	{
		await _operatorIndustries.AddAsync(entity, cancellationToken);
		await SaveAsync(cancellationToken);
	}
	public async Task DeleteAsync(OperatorIndustry industry, CancellationToken cancellationToken)
	{
		_operatorIndustries.Remove(industry);
		await SaveAsync(cancellationToken);
	}
	public IQueryable<OperatorIndustry> GetAll() => _operatorIndustries;
	public async Task<OperatorIndustry?> GetByIdOrDefaultAsync(int industryId) => await _operatorIndustries.SingleOrDefaultAsync(oper => oper.Id == industryId);
	public async Task<OperatorIndustry> GetByIdAsync(int operatorId, int industryId, CancellationToken cancellationToken) => await _operatorIndustries.SingleAsync(oper => 
		oper.Id == industryId && oper.OperatorId == operatorId, cancellationToken: cancellationToken);

	public async Task<List<OperatorIndustry>> GetOperatorsIndustriesAsync(int operatorId, CancellationToken cancellationToken) => await GetAll()
		.Where(i => i.OperatorId == operatorId).ToListAsync(cancellationToken: cancellationToken);

	private async Task SaveAsync(CancellationToken cancellationToken) => await _context.SaveChangesAsync(cancellationToken);
	public async Task UpdateAsync(OperatorIndustry entity, CancellationToken cancellationToken)
	{
		_operatorIndustries.Update(entity);
		await SaveAsync(cancellationToken);
	}
}
