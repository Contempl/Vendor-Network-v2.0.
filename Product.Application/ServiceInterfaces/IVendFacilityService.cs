using Product.Application.Dto;
using Product.Domain.Entity;
using Product.Domain.Result;

namespace Product.Application.ServiceInterfaces;

public interface IVendFacilityService
{
    Task<Response<VendorFacility>> GetFacilityWithServicesByIdAsync(int vendorFacilityId, CancellationToken cancellationToken);
    Task<Response<VendorFacility>> AddFacilityAsync(VendorFacilityDto facilityData, CancellationToken cancellationToken);
    Task<Response<VendorFacility>> UpdateFacilityAsync(int facilityId, UpdateVendorFacilityDto facilityData, CancellationToken cancellationToken);
    Task<Response<int>> RemoveFacilityAsync(int vendorId, int facilityId, CancellationToken cancellationToken);
    Task<Response<VendorFacilityService>> GetVendorFacilityServiceAsync(int facilityId, int facilityServiceId, CancellationToken cancellationToken);
}
