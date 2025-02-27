﻿using Product.Application.Dto;
using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;

namespace Product.Infrastructure.Implementations;

public class VendFacilityService : IVendFacilityService
{
	private readonly IVendorFacilityRepository _vendorFacilityRepository;

	public VendFacilityService(IVendorFacilityRepository vendorFacilityRepository)
	{
		_vendorFacilityRepository = vendorFacilityRepository;
	}

	public async Task CreateAsync(VendorFacility vendorFacility) => await _vendorFacilityRepository.CreateAsync(vendorFacility);
	public async Task DeleteAsync(VendorFacility vendorFacility) => await _vendorFacilityRepository.DeleteAsync(vendorFacility);
	public async Task<VendorFacility> GetByIdAsync(int vendorFacilityId, int vendorId)  => await _vendorFacilityRepository.GetByIdAsync(vendorFacilityId, vendorId);
	public async Task<VendorFacility> GetFacilityWithServicesByIdAsync(int vendorFacilityId, int vendorId)
	{
		return await _vendorFacilityRepository.GetFacilityWithServicesByIdAsync(vendorFacilityId, vendorId);
	}
	public async Task UpdateAsync(VendorFacility vendorFacility) => await _vendorFacilityRepository.UpdateAsync(vendorFacility);
	public VendorFacility MapVendorFacilityFromDtoToCreateAsync(Vendor vendor, VendorFacilityDto facilityData) => new VendorFacility
	{
		Name = facilityData.Name,
		VendorId = vendor.Id,
		Vendor = vendor,
		Location = facilityData.Location,
		Longitude = facilityData.Longitude,
		Latitude = facilityData.Latitude,
		RadiusOfWork = facilityData.RadiusOfWork,
		Services = facilityData.Services.Select(serviceName => new VendorFacilityService { Name = serviceName })
				.ToList(),
	};
	public async Task MapAndUpdateVendorFacility(VendorFacility facility, UpdateVendorFacilityDto facilityData)
	{
		facility.Name = facilityData.Name ?? facility.Name;
		facility.Location = facilityData.Location ?? facility.Location;
		facility.Latitude = facilityData.Latitude ?? facility.Latitude;
		facility.Longitude = facilityData.Longitude ?? facility.Longitude;
		facility.RadiusOfWork = facilityData.RadiusOfWork ?? facility.RadiusOfWork;
		
		if (facilityData.Services?.Any() == true)
		{
			var existingServices = facility.Services.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);
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

		await UpdateAsync(facility);
	}
}
