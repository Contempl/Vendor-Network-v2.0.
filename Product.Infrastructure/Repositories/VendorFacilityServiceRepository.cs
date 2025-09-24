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
	public Task DeleteAsync(VendorFacilityService facilityService, CancellationToken cancellationToken)
	{
		_facilityServices.Remove(facilityService);
		return SaveAsync(cancellationToken);
	}
	public IQueryable<VendorFacilityService> GetAll() => _facilityServices;
	public Task<List<VendorFacilityService>> GetServicesByFacilityIdAsync(int vendorId, int facilityId, CancellationToken cancellationToken)
	{
		var facilityServices =  _facilityServices
			.Where(f => f.VendorFacilityId == facilityId && f.VendorFacility.VendorId == vendorId)
			.ToListAsync(cancellationToken: cancellationToken);
		return facilityServices;
	}
	public Task<VendorFacilityService?> GetByIdOrDefaultAsync(int facilityServiceId) => _facilityServices
		.SingleOrDefaultAsync(facilityService => facilityService.Id == facilityServiceId);
	public Task<VendorFacilityService> GetByIdAsync(int vendorId, int facilityId, int vendorFacilityServiceId, CancellationToken cancellationToken) => 
		_facilityServices.SingleAsync(vfs => vfs.Id == vendorFacilityServiceId
		&& vfs.VendorFacilityId == facilityId 
		&& vfs.VendorFacility.VendorId == vendorId, cancellationToken);

	private Task SaveAsync(CancellationToken cancellationToken) => _context.SaveChangesAsync(cancellationToken);
	public Task UpdateAsync(VendorFacilityService facilityService, CancellationToken cancellationToken)
	{
		_facilityServices.Update(facilityService);
		return SaveAsync(cancellationToken);
	}
}
