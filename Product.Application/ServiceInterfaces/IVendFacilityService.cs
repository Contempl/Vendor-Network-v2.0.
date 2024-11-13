using Product.Domain.Entity;
using Product.Infrastructure.Dto;

namespace Product.Application.ServiceInterfaces;

public interface IVendFacilityService
{
    Task CreateAsync(VendorFacility vendorFacility);
    Task<VendorFacility> GetByIdAsync(int vendorFacilityId, int vendorId);
    Task<VendorFacility> GetFacilityWithServicesByIdAsync(int vendorFacilityId, int vendorId);
	Task UpdateAsync(VendorFacility vendorFacility);
    Task DeleteAsync(VendorFacility vendorFacility);
    VendorFacility MapVendorFacilityFromDtoToCreateAsync(Vendor vendor, VendorFacilityDto facilityData);
    Task MapAndUpdateVendorFacility(VendorFacility facility, UpdateVendorFacilityDto facilityData);
}
