using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;
using Product.Infrastructure.Dto;
using Product.Infrastructure.FIlters;
using Product.WebApi.Pagination;

namespace Product.WebApi.Controllers;

[Route("[controller]")]
[ApiController]
public class OperatorController : Controller
{
	private readonly IOperatorService _operatorService;
	private readonly IOperatorUserService _operatorUserService;

	public OperatorController(IOperatorService operatorService, IOperatorUserService operatorUserService)
	{
		_operatorService = operatorService;
		_operatorUserService = operatorUserService;
	}

	[HttpPost("search/")]
	[Authorize(policy: "OperatorUser")]
	public async Task<IActionResult> GetVendorsToServeFacilities(string serviceType,
	   [FromBody] List<int> operatorLocationsId)
	{
		_operatorService.ValidateStringInput(serviceType);

		var operatorFacilities = _operatorService.GetAllOperatorIndustries(operatorLocationsId);

		var vendors = await _operatorService.SearchVendorsAsync(serviceType, operatorFacilities);

		return Ok(vendors);
	}

	[HttpGet("search/{vendorName}/{pageSize}/{pageNumber}")]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<PagedList<Vendor>>> GetVendors(string vendorName,
		SortOrder sortOrder = SortOrder.Ascending, int pageSize = 10, int pageNumber = 1)
	{
		_operatorService.ValidateStringInput(vendorName);

		var pagedVendors = await _operatorService.GetVendorsQuery(vendorName, sortOrder,
			pageSize, pageNumber);

		return Ok(new PagedList<Vendor>(pagedVendors.Items, pageNumber, pageSize, pagedVendors.TotalCount));
	}


	[HttpGet("{operatorId}")]
	[EnsureOperatorExists]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<Operator>> GetOperator(int operatorId)
	{
		var @operator = await _operatorService.GetByIdAsync(operatorId);

		return Ok(@operator);
	}

	[HttpPost("register/{operatorId}")]
	[EnsureOperatorUserExists]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<Operator>> RegisterOperator(int operatorId, [FromBody] OperatorRegistrationDto operatorRegistrationData)
	{
		var user = await _operatorUserService.GetByIdAsync(operatorId);

		var newOperator = _operatorService.MapOperatorFromDto(operatorRegistrationData, user);

		await _operatorService.CreateAsync(newOperator);
		return CreatedAtAction(nameof(GetOperator), new { operatorId = newOperator.Id }, newOperator);
	}

	[HttpPut("{operatorId}")]
	[EnsureOperatorExists]
	[Authorize(policy: "OperatorUser")]
	public async Task<IActionResult> UpdateOperator(int operatorId, [FromBody] UpdateOperatorDto operatorData)
	{
		var @operator = await _operatorService.GetByIdAsync(operatorId);

		_operatorService.MapOperatorFromDto(@operator, operatorData);

		await _operatorService.UpdateAsync(@operator);

		return Ok(@operator);
	}

	[HttpDelete("{operatorId}")]
	[EnsureOperatorExists]
	[Authorize(policy: "AdminOnly")]
	public async Task<IActionResult> DeleteOperator(int operatorId)
	{
		var @operator = await _operatorService.GetByIdAsync(operatorId);

		await _operatorService.DeleteAsync(@operator);

		return NoContent();
	}
}
