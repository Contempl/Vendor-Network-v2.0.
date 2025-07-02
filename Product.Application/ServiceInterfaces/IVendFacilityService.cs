using Product.Application.Dto;
using Product.Domain.Entity;
using Product.Domain.Result;

namespace Product.Application.ServiceInterfaces;

public interface IVendFacilityService
{
    Task<Response<VendorFacility>> GetFacilityWithServicesByIdAsync(int vendorFacilityId);
    Task<Response<VendorFacility>> AddFacilityAsync(VendorFacilityDto facilityData);
    Task<Response<VendorFacility>> UpdateFacilityAsync(int facilityId, UpdateVendorFacilityDto facilityData);
    Task<Response<int>> RemoveFacilityAsync(int vendorId, int facilityId);
    Task<Response<VendorFacilityService>> GetVendorFacilityServiceAsync(int facilityId, int facilityServiceId);
}
