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

	public async Task CreateAsync(VendorFacilityService facilityService, CancellationToken cancellationToken)
	{
		await _facilityServices.AddAsync(facilityService, cancellationToken);
		await SaveAsync(cancellationToken);
	}
	public async Task DeleteAsync(VendorFacilityService facilityService, CancellationToken cancellationToken)
	{
		_facilityServices.Remove(facilityService);
		await SaveAsync(cancellationToken);
	}
	public IQueryable<VendorFacilityService> GetAll() => _facilityServices;
	public async Task<List<VendorFacilityService>> GetServicesByFacilityIdAsync(int vendorId, int facilityId, CancellationToken cancellationToken)
	{
		var facilityServices = await _facilityServices
			.Where(f => f.VendorFacilityId == facilityId && f.VendorFacility.VendorId == vendorId)
			.ToListAsync(cancellationToken: cancellationToken);
		return facilityServices;
	}
	public async Task<VendorFacilityService?> GetByIdOrDefaultAsync(int facilityServiceId) => await _facilityServices
		.SingleOrDefaultAsync(facilityService => facilityService.Id == facilityServiceId);
	public async Task<VendorFacilityService> GetByIdAsync(int vendorId, int facilityId, int vendorFacilityServiceId, CancellationToken cancellationToken) => await _facilityServices.SingleAsync(vfs => vfs.Id == vendorFacilityServiceId
		&& vfs.VendorFacilityId == facilityId && vfs.VendorFacility.VendorId == vendorId, cancellationToken);

	private async Task SaveAsync(CancellationToken cancellationToken) => await _context.SaveChangesAsync(cancellationToken);
	public async Task UpdateAsync(VendorFacilityService facilityService, CancellationToken cancellationToken)
	{
		_facilityServices.Update(facilityService);
		await SaveAsync(cancellationToken);
	}
}
