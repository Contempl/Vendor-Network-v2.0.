using OneOf;
using OneOf.Types;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Result;

namespace Product.Application.ServiceInterfaces;

public interface IFacilityService
{
    Task<OneOf<VendorFacilityService, NotFoundError, Error>> AddFacilityServiceAsync(int facilityId, string serviceName,
        CancellationToken cancellationToken);
    Task<OneOf<VendorFacilityService, ValidationError, Error>> UpdateFacilityServiceAsync(int facilityId, int facilityServiceId, VendorFacilityServiceDto facilityServiceDto, CancellationToken cancellationToken);
    Task<OneOf<int, Error>> RemoveFacilityServiceAsync(int facilityId, int facilityServiceId, CancellationToken cancellationToken);
}
