using Microsoft.EntityFrameworkCore;
using Product.Application.Interfaces;
using Product.Domain.Entity;

namespace Product.Infrastructure.Repositories;

public class VendorFacilityRepository : IVendorFacilityRepository
{
	private readonly AppDbContext _context;
	private readonly DbSet<VendorFacility> _vendorFacilities;

	public VendorFacilityRepository(AppDbContext context)
	{
		_context = context;
		_vendorFacilities = _context.VendorFacilities;
	}

	public async Task CreateAsync(VendorFacility entity, CancellationToken cancellationToken = default)
	{
		await _vendorFacilities.AddAsync(entity, cancellationToken);
		await SaveAsync(cancellationToken);
	}
	public Task DeleteAsync(VendorFacility vendorFacility, CancellationToken cancellationToken = default)
	{
		_vendorFacilities.Remove(vendorFacility);
		return SaveAsync(cancellationToken);
	}
	public IQueryable<VendorFacility> GetAll() => _vendorFacilities;
	public Task<VendorFacility?> GetByIdOrDefaultAsync(int facilityId) => 
		_vendorFacilities.SingleOrDefaultAsync(vf => vf.Id == facilityId);
	public Task<VendorFacility> GetByIdAsync(int vendorId, int facilityId, CancellationToken cancellationToken = default) =>
		_vendorFacilities.SingleAsync(vf => vf.Id == facilityId 
		                                    && vf.VendorId == vendorId, cancellationToken: cancellationToken);
	public Task<VendorFacility> GetFacilityWithServicesByIdAsync(int facilityId, int vendorId, 
		CancellationToken cancellationToken = default)
	{
		var vendorFacility = _vendorFacilities
			.Where(vf => vf.Id == facilityId)
			.Include(vf => vf.Services)
			.FirstAsync(cancellationToken: cancellationToken);

		return vendorFacility;
	}

	private Task SaveAsync(CancellationToken cancellationToken = default) => 
		_context.SaveChangesAsync(cancellationToken);
	
	public Task UpdateAsync(VendorFacility vendorFacility, CancellationToken cancellationToken = default)
	{
		_vendorFacilities.Update(vendorFacility);
		return SaveAsync(cancellationToken);
	}
}
