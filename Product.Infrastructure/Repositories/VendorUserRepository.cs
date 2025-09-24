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

	public async Task CreateAsync(VendorUser entity, CancellationToken cancellationToken)
	{
		await _vendorUsers.AddAsync(entity, cancellationToken);
		await SaveAsync(cancellationToken);
	}
	public Task DeleteAsync(VendorUser vendorUser, CancellationToken cancellationToken)
	{
		_vendorUsers.Remove(vendorUser);
		return SaveAsync(cancellationToken);
	}
	public IQueryable<VendorUser> GetAll() => _vendorUsers;
	public Task<VendorUser?> GetByIdOrDefaultAsync(int id) => _vendorUsers.SingleOrDefaultAsync(vu => vu.Id == id);
	public Task<VendorUser> GetByIdAsync(int id, CancellationToken cancellationToken) => _vendorUsers.SingleAsync(vu => vu.Id == id, cancellationToken: cancellationToken);
	public Task UpdateAsync(VendorUser vendorUser, CancellationToken cancellationToken)
	{
		var userToUpdate = _vendorUsers.FindAsync(vendorUser.Id, cancellationToken).Result;

		_context.Entry(userToUpdate).CurrentValues.SetValues(vendorUser);
		return SaveAsync(cancellationToken);
	}

	private Task SaveAsync(CancellationToken cancellationToken) => _context.SaveChangesAsync(cancellationToken);
}
