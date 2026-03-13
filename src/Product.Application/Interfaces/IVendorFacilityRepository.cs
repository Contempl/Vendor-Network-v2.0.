using Product.Domain.Entity;

namespace Product.Application.Interfaces;

public interface IVendorFacilityRepository : IRepository<VendorFacility>
{
	Task<VendorFacility> GetByIdAsync(int facilityId, int vendorId, CancellationToken cancellationToken);
	Task<VendorFacility> GetFacilityWithServicesByIdAsync(int vendorFacilityId, int vendorId, CancellationToken cancellationToken);
}