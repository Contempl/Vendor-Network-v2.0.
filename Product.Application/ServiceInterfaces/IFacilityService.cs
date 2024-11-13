using Product.Domain.Entity;

namespace Product.Application.ServiceInterfaces;

public interface IFacilityService
{
    Task CreateAsync(VendorFacilityService facilityService);
    Task<VendorFacilityService> GetByIdAsync(int facilityServiceId, int facilityId, int vendorId);
    Task UpdateAsync(VendorFacilityService facilityService);
    Task DeleteAsync(VendorFacilityService facilityService);
    Task<List<VendorFacilityService>> GetServicesByFacilityIdAsync(int facilityId, int vendorId);
    VendorFacilityService MapFacilityServiceDtoToCreate(VendorFacility facility, string serviceName);
    void ValidateServiceName(string serviceName);
    void UpdateFacilityServiceName(VendorFacilityService facilityService, string serviceName);
}
