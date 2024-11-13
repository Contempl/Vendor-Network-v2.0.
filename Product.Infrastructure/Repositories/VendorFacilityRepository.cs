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

	public async Task CreateAsync(VendorFacility entity)
	{
		await _vendorFacilities.AddAsync(entity);
		await SaveAsync();
	}
	public async Task DeleteAsync(VendorFacility vendorFacility)
	{
		_vendorFacilities.Remove(vendorFacility);
		await SaveAsync();
	}
	public IQueryable<VendorFacility> GetAll() => _vendorFacilities;
	public async Task<VendorFacility?> GetByIdOrDefaultAsync(int facilityId) => await _vendorFacilities.SingleOrDefaultAsync(vf => vf.Id == facilityId);
	public async Task<VendorFacility> GetByIdAsync(int vendorId, int facilityId) => await _vendorFacilities.SingleAsync(vf => vf.Id == facilityId && vf.VendorId == vendorId);
	public async Task<VendorFacility> GetFacilityWithServicesByIdAsync(int vendorId, int facilityId)
	{
		var vendorFacility = await _vendorFacilities
			.Include(vf => vf.Services)
			.SingleAsync(vf => vf.Id == facilityId && vf.VendorId == vendorId);

		return vendorFacility;
	}
	public async Task SaveAsync() => await _context.SaveChangesAsync();
	public async Task UpdateAsync(VendorFacility vendorFacility)
	{
		_vendorFacilities.Update(vendorFacility);
		await SaveAsync();
	}
}
