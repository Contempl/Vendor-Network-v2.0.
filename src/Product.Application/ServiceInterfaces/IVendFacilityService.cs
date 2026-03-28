using OneOf;
using OneOf.Types;
using Product.Application.Dto;
using Product.Domain.Entity;
using Product.Domain.Result;

namespace Product.Application.ServiceInterfaces;

public interface IVendFacilityService
{
    Task<OneOf<VendorFacility, Error>> GetFacilityWithServicesByIdAsync(int vendorFacilityId, CancellationToken cancellationToken);
    Task<OneOf<VendorFacility, NotFoundError, Error>> AddFacilityAsync(VendorFacilityDto facilityData, CancellationToken cancellationToken);
    Task<OneOf<VendorGetFacilitiesDto, Error>> UpdateFacilityAsync(int facilityId, UpdateVendorFacilityDto facilityData, CancellationToken cancellationToken);
    Task<OneOf<int, Error>> RemoveFacilityAsync(int vendorId, int facilityId, CancellationToken cancellationToken);
    Task<OneOf<VendorFacilityService, NotFoundError, Error>> GetVendorFacilityServiceAsync(int facilityId, int facilityServiceId, CancellationToken cancellationToken);
}
