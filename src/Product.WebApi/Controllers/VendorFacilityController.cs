using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Types;
using Product.Application.Dto;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Enum;
using Product.Domain.Result;
using Product.Infrastructure.Filters;

namespace Product.WebApi.Controllers;

[Route("vendor")]
[ApiController]
public class VendorFacilityController : ControllerBase
{
	private readonly IVendFacilityService _vendorFacilityService;
	private readonly IFacilityService _facilityService;

	public VendorFacilityController(IVendFacilityService vendorFacilityService, IFacilityService facilityService)
	{
		_vendorFacilityService = vendorFacilityService;
		_facilityService = facilityService;
	}

	[HttpGet("/facility/{facilityId}")]
	[EnsureVendorFacilityExists]
	[EnsureBusinessAccess(UserType.VendorUser)]
	[Authorize(policy: "VendorUser")]
	public async Task<ActionResult<OneOf<VendorFacility, Error>>> GetVendorFacility(int facilityId, 
		CancellationToken cancellationToken)
	{
		var response = await _vendorFacilityService.GetFacilityWithServicesByIdAsync(facilityId, cancellationToken);
		
		return response.Match<ActionResult>(
			facility => Ok(facility),
			error => StatusCode(500, error));
	}

	[HttpPost("/facility")]
	[EnsureBusinessAccess(UserType.VendorUser)]
	[Authorize(policy: "VendorUser")]
	public async Task<ActionResult<OneOf<VendorFacility, NotFoundError, Error>>> AddFacility(VendorFacilityDto facilityData, 
		CancellationToken cancellationToken)
	{
		var response = await _vendorFacilityService.AddFacilityAsync(facilityData, cancellationToken);
		
		return response.Match<ActionResult>(
			facility => Ok(facility),
			notFound => BadRequest(notFound),
			error => StatusCode(500, error));
	}

	[HttpPut("facility/{facilityId}")]
	[EnsureVendorFacilityExists]
	[EnsureBusinessAccess(UserType.VendorUser)]
	[Authorize(policy: "VendorUser")]
	public async Task<ActionResult<OneOf<VendorFacility, Error>>> UpdateFacility(int facilityId,
		[FromBody] UpdateVendorFacilityDto facilityData, CancellationToken cancellationToken)
	{
		var response = await _vendorFacilityService.UpdateFacilityAsync(facilityId, facilityData, cancellationToken);
		
		return response.Match<ActionResult>(
			facility => Ok(facility),
			error => StatusCode(500, error));
	}

	[HttpDelete("{vendorId}/facilities/{facilityId}")]
	[EnsureVendorFacilityExists] 
	[Authorize(policy: "VendorUser")] 
	public async Task<ActionResult<OneOf<int, Error>>> DeleteFacility(int vendorId, int facilityId,
		CancellationToken cancellationToken)
	{
		var response = await _vendorFacilityService.RemoveFacilityAsync(vendorId, facilityId, cancellationToken);
		
		return response.Match<ActionResult>(
			facilityId => Ok(facilityId),
			error => StatusCode(500, error));
	}


	[HttpGet("/facility/{facilityId}/service/{facilityServiceId}")]
	[EnsureBusinessAccess(UserType.VendorUser)]
	[Authorize(policy: "VendorUser")]
	public async Task<ActionResult<OneOf<VendorFacilityService, NotFoundError, Error>>> GetVendorFacilityService(int facilityId, 
		int facilityServiceId, CancellationToken cancellationToken)
	{
		var response = await _vendorFacilityService.GetVendorFacilityServiceAsync(facilityId, facilityServiceId, cancellationToken);
		
		return response.Match<ActionResult>(
			facility => Ok(facility),
			notFound => BadRequest(notFound),
			error => StatusCode(500, error));
		
	}

	[HttpGet("/facility/{facilityId}/services")]
	[EnsureBusinessAccess(UserType.VendorUser)]
	[Authorize(policy: "VendorUser")]
	public async Task<ActionResult<OneOf<List<VendorFacilityService>, Error>>> GetVendorFacilityServices(int facilityId,
		CancellationToken cancellationToken)
	{
		var response = await _vendorFacilityService.GetFacilityWithServicesByIdAsync(facilityId, cancellationToken);
		
		return response.Match<ActionResult>(
			facility => Ok(facility),
			error => StatusCode(500, error));
	}

	[HttpPost("/facility/{facilityId}/service")]
	[EnsureVendorFacilityExists]
	[EnsureBusinessAccess(UserType.VendorUser)]
	[Authorize(policy: "VendorUser")]
	public async Task<ActionResult<OneOf<VendorFacilityService, NotFoundError, Error>>> AddFacilityService(int facilityId, 
		[FromBody] string facilityServiceName, CancellationToken cancellationToken)
	{
		var response = await _facilityService.AddFacilityServiceAsync(facilityId, facilityServiceName, cancellationToken);
		
		return response.Match<ActionResult>(
			service => Ok(service),
			notFound => BadRequest(notFound),
			error => StatusCode(500, error));
	}

	[HttpPut("/facility/{facilityId}/service/{facilityServiceId}")]
	[EnsureVendorFacilityServiceExists]
	[EnsureBusinessAccess(UserType.VendorUser)]
	[Authorize(policy: "VendorUser")]
	public async Task<ActionResult<OneOf<VendorFacilityService, ValidationError, Error>>> UpdateFacilityService(int facilityId, 
		int facilityServiceId, [FromBody] VendorFacilityServiceDto facilityServiceDto, 
		CancellationToken cancellationToken)
	{
		var response = await _facilityService.UpdateFacilityServiceAsync(facilityId, facilityServiceId, 
			facilityServiceDto, cancellationToken);
		
		return response.Match<ActionResult>(
			service => Ok(service),
			validationError => BadRequest(validationError),
			error => StatusCode(500, error));
	}

	[HttpDelete("/facility/{facilityId}/service/{facilityServiceId}")]
	[EnsureVendorFacilityServiceExists]
	[EnsureBusinessAccess(UserType.VendorUser)]
	[Authorize(policy: "VendorUser")]
	public async Task<ActionResult<OneOf<int, Error>>> DeleteFacilityService(int facilityId, 
		int facilityServiceId, CancellationToken cancellationToken)
	{
		var response = await _facilityService.RemoveFacilityServiceAsync(facilityId, facilityServiceId, cancellationToken);

		return response.Match<ActionResult>(
			serviceId => Ok(serviceId),
			error => StatusCode(500, error));
	}
}
