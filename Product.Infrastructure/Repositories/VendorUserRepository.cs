using Microsoft.EntityFrameworkCore;
using Product.Application.Interfaces;
using Product.Domain.Entity;

namespace Product.Infrastructure.Repositories;

public class VendorUserRepository : IVendorUserRepository
{
	private readonly AppDbContext _context;
	private readonly DbSet<VendorUser> _vendorUsers;

	public VendorUserRepository(AppDbContext context)
	{
		_context = context;
		_vendorUsers = _context.VendorUsers;
	}

	public async Task CreateAsync(VendorUser entity)
	{
		await _vendorUsers.AddAsync(entity);
		await SaveAsync();
	}
	public async Task DeleteAsync(VendorUser vendorUser)
	{
		_vendorUsers.Remove(vendorUser);
		await SaveAsync();
	}
	public IQueryable<VendorUser> GetAll() => _vendorUsers;
	public async Task<VendorUser?> GetByIdOrDefaultAsync(int id) => await _vendorUsers.SingleOrDefaultAsync(vu => vu.Id == id);
	public async Task<VendorUser> GetByIdAsync(int id) => await _vendorUsers.SingleAsync(vu => vu.Id == id);
	public async Task UpdateAsync(VendorUser vendorUser)
	{
		_vendorUsers.Update(vendorUser);
		await SaveAsync();
	}
	public async Task SaveAsync() => await _context.SaveChangesAsync();
}
