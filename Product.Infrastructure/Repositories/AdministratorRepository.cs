using Microsoft.EntityFrameworkCore;
using Product.Application.Interfaces;
using Product.Domain.Entity;

namespace Product.Infrastructure.Repositories;

public class AdministratorRepository : IAdministratorRepository
{
	private readonly AppDbContext _context;
	private readonly DbSet<Administrator> _administrators;

	public AdministratorRepository(AppDbContext context)
	{
		_context = context;
		_administrators = _context.Administrators;
	}
	public async Task CreateAsync(Administrator admin)
	{
		await _administrators.AddAsync(admin);
		await SaveAsync();
	}
	public async Task DeleteAsync(Administrator admin)
	{
		_administrators.Remove(admin);
		await SaveAsync();
	}
	public IQueryable<Administrator> GetAll() => _administrators;
	public async Task<Administrator?> GetByIdOrDefaultAsync(int AdminId) => await _administrators.SingleOrDefaultAsync(admin => admin.Id == AdminId);
	public async Task<Administrator> GetByIdAsync(int AdminId) => await _administrators.SingleAsync(admin => admin.Id == AdminId);
	public Task SaveAsync() => _context.SaveChangesAsync();
	public async Task UpdateAsync(Administrator admin)
	{
		_administrators.Update(admin);
		await SaveAsync();
	}
}
