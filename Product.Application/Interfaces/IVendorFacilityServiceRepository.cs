using Product.Domain.Entity;

namespace Product.Application.Interfaces;

public interface IVendorFacilityServiceRepository : IRepository<VendorFacilityService>
{
	Task<VendorFacilityService> GetByIdAsync(int facilityServiceId, int facilityId, int vendorId);
	Task<List<VendorFacilityService>> GetServicesByFacilityIdAsync(int facilityId, int vendorId);
}