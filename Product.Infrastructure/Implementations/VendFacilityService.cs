using Product.Application.Dto;
using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;
using Product.Domain.Enum;
using Product.Domain.Result;

namespace Product.Infrastructure.Implementations;

public class VendFacilityService : IVendFacilityService
{
	private readonly IVendorFacilityRepository _vendorFacilityRepository;
	private readonly IUserPrincipalService _userPrincipalService;
	private readonly IVendorRepository _vendorRepository;

	public VendFacilityService(IVendorFacilityRepository vendorFacilityRepository, 
		IUserPrincipalService userPrincipalService, IVendorRepository vendorRepository)
	{
		_vendorFacilityRepository = vendorFacilityRepository;
		_userPrincipalService = userPrincipalService;
		_vendorRepository = vendorRepository;
	}
	public async Task<Response<VendorFacility>> GetFacilityWithServicesByIdAsync(int vendorFacilityId, 
		CancellationToken cancellationToken = default)
	{
		var vendorId = _userPrincipalService.BusinessId!.Value;
		var result = await _vendorFacilityRepository.GetFacilityWithServicesByIdAsync(vendorFacilityId, vendorId, cancellationToken);

		return new Response<VendorFacility>
		{
			Data = result,
		};
	}
	private VendorFacility MapVendorFacilityFromDtoToCreate(Vendor vendor, VendorFacilityDto facilityData) => new VendorFacility
	{
		Name = facilityData.Name,
		VendorId = vendor.Id,
		Location = facilityData.Location,
		Longitude = facilityData.Longitude,
		Latitude = facilityData.Latitude,
		RadiusOfWork = facilityData.RadiusOfWork,
		Services = facilityData.Services.Select(serviceName => new VendorFacilityService { Name = serviceName })
				.ToList(),
	};
	private void MapAndUpdateVendorFacility(VendorFacility facility, UpdateVendorFacilityDto facilityData)
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
	

	public async Task<Response<VendorFacility>> AddFacilityAsync(VendorFacilityDto facilityData, 
		CancellationToken cancellationToken = default)
	{
		if (facilityData.Services is null || facilityData.Services.Any() == false)
		{
			return new Response<VendorFacility>()
			{
				ErrorCode = (int)ErrorCodes.InvalidVendorFacilityData,
				ErrorMessage = "No services provided for vendor facility creation",
			};
		}
		var vendorId = _userPrincipalService.BusinessId!.Value;
		var vendor = await _vendorRepository.GetByIdAsync(vendorId, cancellationToken);

		var facility = MapVendorFacilityFromDtoToCreate(vendor, facilityData);
		
		await _vendorFacilityRepository.CreateAsync(facility, cancellationToken);
		
		return new Response<VendorFacility>
		{
			Data = facility,
		};
	}

	public async Task<Response<VendorFacility>> UpdateFacilityAsync(int facilityId, UpdateVendorFacilityDto facilityData, 
		CancellationToken cancellationToken = default)
	{
		var vendorId = _userPrincipalService.BusinessId!.Value;
		var facility = await _vendorFacilityRepository.GetFacilityWithServicesByIdAsync(facilityId, vendorId, cancellationToken);

		MapAndUpdateVendorFacility(facility, facilityData);
		
		await _vendorFacilityRepository.UpdateAsync(facility,cancellationToken);
	
		return new Response<VendorFacility>
		{
			Data = facility,
		};
	}

	public async Task<Response<int>> RemoveFacilityAsync(int vendorId, int facilityId, 
		CancellationToken cancellationToken = default)
	{
		var vendorFacility = await _vendorFacilityRepository.GetByIdAsync(vendorId, facilityId, cancellationToken);

		await _vendorFacilityRepository.DeleteAsync(vendorFacility, cancellationToken);

		return new Response<int>
		{
			Data = vendorFacility.Id,
		};
	}

	public async Task<Response<VendorFacilityService>> GetVendorFacilityServiceAsync(int facilityId, int facilityServiceId, 
		CancellationToken cancellationToken = default)
	{
		var vendorId = _userPrincipalService.BusinessId!.Value;
		var vendorFacility = await _vendorFacilityRepository.GetFacilityWithServicesByIdAsync(facilityId, vendorId, cancellationToken);
		
		var result =  vendorFacility switch
		{
			null => new Response<VendorFacilityService>()
			{
				ErrorMessage = "Couldn't fetch vendor facility",
				ErrorCode = (int)ErrorCodes.InvalidVendorFacilityData,
			},
			{ Services: var services } => services.FirstOrDefault(vfs => vfs.Id == facilityServiceId) switch
			{
				null => new Response<VendorFacilityService>()
				{
					ErrorMessage = "Couldn't fetch vendor facility service",
					ErrorCode = (int)ErrorCodes.InvalidVendorFacilityServiceData
				},
				var facilityService => new Response<VendorFacilityService>
				{
					Data = facilityService
				}
			},
		};
		return result;
	}
}
