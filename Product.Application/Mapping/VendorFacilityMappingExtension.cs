using Product.Application.Dto;
using Product.Domain.Entity;

namespace Product.Application.Mapping;

public static class VendorFacilityMappingExtension
{
    public static  void MapAndUpdateVendorFacility(this VendorFacility facility, UpdateVendorFacilityDto facilityData)
    {
        facility.Name = facilityData.Name ?? facility.Name;
        facility.Location = facilityData.Location ?? facility.Location;
        facility.Latitude = facilityData.Latitude ?? facility.Latitude;
        facility.Longitude = facilityData.Longitude ?? facility.Longitude;
        facility.RadiusOfWork = facilityData.RadiusOfWork ?? facility.RadiusOfWork;
		
        if (facilityData.Services?.Any() == true)
        {
            var existingServices = facility.Services
                .ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);
			
            var updatedServices = new List<VendorFacilityService>();

            foreach (var serviceName in facilityData.Services.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (existingServices.TryGetValue(serviceName, out var existingService))
                {
                    updatedServices.Add(existingService);
                    existingServices.Remove(serviceName);
                }
                else
                {
                    updatedServices.Add(new VendorFacilityService 
                    { 
                        Name = serviceName,
                        VendorFacilityId = facility.Id
                    });
                }
            }
            facility.Services.Clear();
            facility.Services.AddRange(updatedServices);
        }
    }
}