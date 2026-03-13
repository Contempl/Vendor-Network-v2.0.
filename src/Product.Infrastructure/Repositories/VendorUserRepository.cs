using Microsoft.EntityFrameworkCore;
using Product.Application.Dto;
using Product.Application.Interfaces;
using Product.Domain.Entity;
using Product.Infrastructure.Extensions;

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
	
	public async Task CreateAsync(VendorUser entity, CancellationToken cancellationToken = default)
	{
		await _vendorUsers.AddAsync(entity, cancellationToken);
		await SaveAsync(cancellationToken);
	}

	public async Task<VendorUser> CreateAsync(VendorUserCreationDto userCreationDto, CancellationToken cancellationToken = default)
	{
		var vendorUser = userCreationDto.MapToVendorUser();
		await _vendorUsers.AddAsync(vendorUser, cancellationToken);
		await SaveAsync(cancellationToken);
		return vendorUser;
	}
	public Task DeleteAsync(VendorUser vendorUser, CancellationToken cancellationToken = default)
	{
		_vendorUsers.Remove(vendorUser);
		return SaveAsync(cancellationToken);
	}
	public IQueryable<VendorUser> GetAll() => _vendorUsers;
	public Task<VendorUser?> GetByIdOrDefaultAsync(int id) => _vendorUsers.SingleOrDefaultAsync(vu => vu.Id == id);
	public Task<VendorUser> GetByIdAsync(int id, CancellationToken cancellationToken = default) => 
		_vendorUsers.SingleAsync(vu => vu.Id == id, cancellationToken: cancellationToken);



	public async Task UpdateAsync(VendorUser vendorUser, CancellationToken cancellationToken = default)
	{
		var userToUpdate = await _vendorUsers.FindAsync(vendorUser.Id, cancellationToken);

		_context.Entry(userToUpdate).CurrentValues.SetValues(vendorUser);
		await SaveAsync(cancellationToken);
	}

	private Task SaveAsync(CancellationToken cancellationToken = default) => 
		_context.SaveChangesAsync(cancellationToken);
}
