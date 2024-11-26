using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.Application.Dto;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;
using Product.Infrastructure.Filters;

namespace Product.WebApi.Controllers;

[Route("vendor")]
[ApiController]
public class VendorFacilityController : ControllerBase
{
	private readonly IVendorService _vendorService;
	private readonly IVendFacilityService _vendorFacilityService;
	private readonly IFacilityService _facilityService;
	private readonly IUserPrincipalService _userPrincipalService;

	public VendorFacilityController(IVendFacilityService vendorFacilityService,
		IFacilityService facilityService, IVendorService vendorService, IUserPrincipalService userPrincipalService)
	{
		_vendorFacilityService = vendorFacilityService;
		_facilityService = facilityService;
		_vendorService = vendorService;
		_userPrincipalService = userPrincipalService;
	}

	[HttpGet("/facility/{facilityId}")]
	[EnsureVendorFacilityExists]
	[EnsureBusinessAccess(nameof(VendorUser))]
	[Authorize(policy: "VendorUser")]
	public async Task<ActionResult<VendorFacility>> GetVendorFacility(int facilityId)
	{
		var vendorId = _userPrincipalService.BusinessId;
		var vendorFacility = await _vendorFacilityService.GetFacilityWithServicesByIdAsync(facilityId, vendorId!.Value);

		return Ok(vendorFacility);
	}

	[HttpPost("/facility")]
	[EnsureBusinessAccess(nameof(VendorUser))]
	[Authorize(policy: "VendorUser")]
	public async Task<IActionResult> AddFacility(VendorFacilityDto facilityData)
	{
		var vendorId = _userPrincipalService.BusinessId;
		var vendor = await _vendorService.GetByIdAsync(vendorId!.Value);

		var facility = _vendorFacilityService.MapVendorFacilityFromDtoToCreateAsync(vendor, facilityData);

		await _vendorFacilityService.CreateAsync(facility);

		return CreatedAtAction(nameof(GetVendorFacility), new { vendorId = vendor.Id, facilityId = facility.Id }, facility);
	}

	[HttpPut("facility/{facilityId}")]
	[EnsureVendorFacilityExists]
	[EnsureBusinessAccess(nameof(VendorUser))]
	[Authorize(policy: "VendorUser")]
	public async Task<IActionResult> UpdateFacility(int facilityId,
		[FromBody] UpdateVendorFacilityDto facilityData)
	{
		var vendorId = _userPrincipalService.BusinessId;
		var facility = await _vendorFacilityService.GetFacilityWithServicesByIdAsync(facilityId, vendorId!.Value);

		await _vendorFacilityService.MapAndUpdateVendorFacility(facility, facilityData);
		return Ok(facility);
	}

	[HttpDelete("facilities/{facilityId}")]
	[EnsureVendorFacilityExists] // не завернёт ли при попытке админа удалить facility? А вендор сам не может удалять свои фасилитис ?
	[Authorize(policy: "AdminOnly")] 
	public async Task<ActionResult> DeleteFacility(int vendorId, int facilityId)
	{
		var vendorFacility = await _vendorFacilityService.GetByIdAsync(vendorId, facilityId);

		await _vendorFacilityService.DeleteAsync(vendorFacility);

		return NoContent();
	}


	[HttpGet("/facility/{facilityId}/service/{facilityServiceId}")]
	[EnsureBusinessAccess(nameof(VendorUser))]
	[Authorize(policy: "VendorUser")]
	public async Task<ActionResult> GetVendorFacilityService(int facilityId, int facilityServiceId)
	{
		var vendorId = _userPrincipalService.BusinessId;
		var vendorFacility = await _vendorFacilityService.GetFacilityWithServicesByIdAsync(facilityId, vendorId!.Value);
		
		return vendorFacility switch
		{
			null => BadRequest("Invalid data"),
			{ Services: var services } => services.FirstOrDefault(vfs => vfs.Id == facilityServiceId) switch
			{
				null => NotFound($"Service not found with id: {facilityServiceId}"),
				var facilityService => Ok(facilityService)
			},
		};
	}

	[HttpGet("/facility/{facilityId}/services")]
	[EnsureBusinessAccess(nameof(VendorUser))]
	[Authorize(policy: "VendorUser")]
	public async Task<ActionResult<List<VendorFacilityService>>> GetVendorFacilityServices(int facilityId)
	{
		var vendorId = _userPrincipalService.BusinessId;
		var facilityServices = await _facilityService.GetServicesByFacilityIdAsync(vendorId!.Value, facilityId);

		return Ok(facilityServices);
	}

	[HttpPost("/facility/{facilityId}/service")]
	[EnsureVendorFacilityExists]
	[EnsureBusinessAccess(nameof(VendorUser))]
	[Authorize(policy: "VendorUser")]
	public async Task<ActionResult> AddFacilityService(int facilityId, [FromBody] string facilityServiceName)
	{
		var vendorId = _userPrincipalService.BusinessId;
		var facility = await _vendorFacilityService.GetByIdAsync(facilityId, vendorId!.Value);

		var newFacilityService = _facilityService.MapFacilityServiceDtoToCreate(facility, facilityServiceName);

		await _facilityService.CreateAsync(newFacilityService);

		return Ok(newFacilityService);
	}

	[HttpPut("/facility/{facilityId}/service/{facilityServiceId}")]
	[EnsureVendorFacilityServiceExists]
	[EnsureBusinessAccess(nameof(VendorUser))]
	[Authorize(policy: "VendorUser")]
	public async Task<ActionResult> UpdateFacilityService(int facilityId, int facilityServiceId, [FromBody] string facilityServiceName)
	{
		var vendorId = _userPrincipalService.BusinessId;
		var vendorFacilityService = await _facilityService.GetByIdAsync(vendorId!.Value, facilityId, facilityServiceId);

		_facilityService.ValidateServiceName(facilityServiceName);

		_facilityService.UpdateFacilityServiceName(vendorFacilityService, facilityServiceName);

		await _facilityService.UpdateAsync(vendorFacilityService);

		return Ok(vendorFacilityService);
	}

	[HttpDelete("/facility/{facilityId}/service/{facilityServiceId}")]
	[EnsureVendorFacilityServiceExists]
	[EnsureBusinessAccess(nameof(VendorUser))]
	[Authorize(policy: "VendorUser")]
	public async Task<ActionResult> DeleteFacilityService(int facilityId, int facilityServiceId)
	{
		var vendorId = _userPrincipalService.BusinessId;
		var facilityService = await _facilityService.GetByIdAsync(vendorId!.Value, facilityId, facilityServiceId);

		await _facilityService.DeleteAsync(facilityService);

		return NoContent();
	}
}
