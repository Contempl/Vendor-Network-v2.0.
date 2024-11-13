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

	public async Task CreateAsync(OperatorIndustry entity)
	{
		await _operatorIndustries.AddAsync(entity);
		await SaveAsync();
	}
	public async Task DeleteAsync(OperatorIndustry industry)
	{
		_operatorIndustries.Remove(industry);
		await SaveAsync();
	}
	public IQueryable<OperatorIndustry> GetAll() => _operatorIndustries;
	public async Task<OperatorIndustry?> GetByIdOrDefaultAsync(int industryId) => await _operatorIndustries.SingleOrDefaultAsync(oper => oper.Id == industryId);
	public async Task<OperatorIndustry> GetByIdAsync(int operatorId, int industryId) => await _operatorIndustries.SingleAsync(oper => oper.Id == industryId && oper.OperatorId == operatorId);
	public async Task SaveAsync() => await _context.SaveChangesAsync();
	public async Task UpdateAsync(OperatorIndustry entity)
	{
		_operatorIndustries.Update(entity);
		await SaveAsync();
	}
}
