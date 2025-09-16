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

	public async Task CreateAsync(VendorFacility entity, CancellationToken cancellationToken)
	{
		await _vendorFacilities.AddAsync(entity, cancellationToken);
		await SaveAsync(cancellationToken);
	}
	public async Task DeleteAsync(VendorFacility vendorFacility, CancellationToken cancellationToken)
	{
		_vendorFacilities.Remove(vendorFacility);
		await SaveAsync(cancellationToken);
	}
	public IQueryable<VendorFacility> GetAll() => _vendorFacilities;
	public async Task<VendorFacility?> GetByIdOrDefaultAsync(int facilityId) => await _vendorFacilities.SingleOrDefaultAsync(vf => vf.Id == facilityId);
	public async Task<VendorFacility> GetByIdAsync(int vendorId, int facilityId, CancellationToken cancellationToken) => await _vendorFacilities.SingleAsync(vf => vf.Id == facilityId && vf.VendorId == vendorId, cancellationToken: cancellationToken);
	public async Task<VendorFacility> GetFacilityWithServicesByIdAsync(int facilityId, int vendorId, CancellationToken cancellationToken)
	{
		var vendorFacility = await _vendorFacilities
			.Where(vf => vf.Id == facilityId)
			.Include(vf => vf.Services)
			.FirstAsync(cancellationToken: cancellationToken);

		return vendorFacility;
	}

	private async Task SaveAsync(CancellationToken cancellationToken) => await _context.SaveChangesAsync(cancellationToken);
	
	public async Task UpdateAsync(VendorFacility vendorFacility, CancellationToken cancellationToken)
	{
		_vendorFacilities.Update(vendorFacility);
		await SaveAsync(cancellationToken);
	}
}
