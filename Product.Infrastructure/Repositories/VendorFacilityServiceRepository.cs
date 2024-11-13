using Microsoft.EntityFrameworkCore;
using Product.Application.Interfaces;
using Product.Domain.Entity;

namespace Product.Infrastructure.Repositories;

public class VendorFacilityServiceRepository : IVendorFacilityServiceRepository
{
	private readonly AppDbContext _context;
	private readonly DbSet<VendorFacilityService> _facilityServices;

	public VendorFacilityServiceRepository(AppDbContext context)
	{
		_context = context;
		_facilityServices = _context.VendorFacilityServices;

	}

	public async Task CreateAsync(VendorFacilityService service)
	{
		await _facilityServices.AddAsync(service);
		await SaveAsync();
	}
	public async Task DeleteAsync(VendorFacilityService facilityService)
	{
		_facilityServices.Remove(facilityService);
		await SaveAsync();
	}
	public IQueryable<VendorFacilityService> GetAll() => _facilityServices;
	public async Task<List<VendorFacilityService>> GetServicesByFacilityIdAsync(int vendorId, int facilityId)
	{
		var facilityServices = await _facilityServices
			.Where(f => f.VendorFacilityId == facilityId && f.VendorFacility.VendorId == vendorId)
			.ToListAsync();
		return facilityServices;
	}
	public async Task<VendorFacilityService?> GetByIdOrDefaultAsync(int facilityServiceId) => await _facilityServices
		.SingleOrDefaultAsync(oper => oper.Id == facilityServiceId);
	public async Task<VendorFacilityService> GetByIdAsync(int vendorId, int facilityId, int facilityServiceId) => await _facilityServices.SingleAsync(vfs => vfs.Id == facilityServiceId
		&& vfs.VendorFacilityId == facilityId && vfs.VendorFacility.VendorId == vendorId);
	public async Task SaveAsync() => await _context.SaveChangesAsync();
	public async Task UpdateAsync(VendorFacilityService facilityService)
	{
		_facilityServices.Update(facilityService);
		await SaveAsync();
	}
}
