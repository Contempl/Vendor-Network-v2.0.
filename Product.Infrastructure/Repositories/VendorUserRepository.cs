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
	public async Task DeleteAsync(VendorUser vendorUser, CancellationToken cancellationToken)
	{
		_vendorUsers.Remove(vendorUser);
		await SaveAsync(cancellationToken);
	}
	public IQueryable<VendorUser> GetAll() => _vendorUsers;
	public async Task<VendorUser?> GetByIdOrDefaultAsync(int id) => await _vendorUsers.SingleOrDefaultAsync(vu => vu.Id == id);
	public async Task<VendorUser> GetByIdAsync(int id, CancellationToken cancellationToken) => await _vendorUsers.SingleAsync(vu => vu.Id == id, cancellationToken: cancellationToken);
	public async Task UpdateAsync(VendorUser vendorUser, CancellationToken cancellationToken)
	{
		var userToUpdate = await _vendorUsers.FindAsync(vendorUser.Id, cancellationToken);

		if (userToUpdate != null)
		{
			_context.Entry(userToUpdate).CurrentValues.SetValues(vendorUser);
			await SaveAsync(cancellationToken);
		}
	}

	private async Task SaveAsync(CancellationToken cancellationToken) => await _context.SaveChangesAsync(cancellationToken);
}
