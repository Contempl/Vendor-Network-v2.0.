using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Result;

namespace Product.Application.ServiceInterfaces;

public interface IFacilityService
{
    Task<Response<VendorFacilityService>> AddFacilityServiceAsync(int facilityId, string serviceName,
        CancellationToken cancellationToken);
    Task<Response<VendorFacilityService>> UpdateFacilityServiceAsync(int facilityId, int facilityServiceId, VendorFacilityServiceDto facilityServiceDto, CancellationToken cancellationToken);
    Task<Response<int>> RemoveFacilityServiceAsync(int facilityId, int facilityServiceId, CancellationToken cancellationToken);
}
