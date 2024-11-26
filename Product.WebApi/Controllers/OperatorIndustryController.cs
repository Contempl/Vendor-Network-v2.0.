using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.Application.Dto;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;
using Product.Infrastructure.Filters;

namespace Product.WebApi.Controllers;

[Route("operator")]
[ApiController]
public class OperatorIndustryController : ControllerBase
{
	private readonly IOperatorIndustryService _operatorIndustryService;
	private readonly IOperatorService _operatorService;
	private readonly IUserPrincipalService _userPrincipalService;

	public OperatorIndustryController(IOperatorIndustryService operatorIndustryService, IOperatorService operatorService, IUserPrincipalService userPrincipalService)
	{
		_operatorIndustryService = operatorIndustryService;
		_operatorService = operatorService;
		_userPrincipalService = userPrincipalService;
	}

	[HttpGet("industry/{industryId}")]
	[EnsureOperatorIndustryExists]
	[EnsureBusinessAccess(nameof(OperatorUser))]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<OperatorIndustry>> GetOperatorFacility(int industryId)
	{
		var operatorId = _userPrincipalService.BusinessId;
		var industry = await _operatorIndustryService.GetByIdAsync(operatorId!.Value, industryId);

		return Ok(industry);
	}

	[HttpGet("industries")]
	[EnsureBusinessAccess(nameof(OperatorUser))]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<List<OperatorIndustry>>> GetOperatorIndustries()
	{
		var operatorId = _userPrincipalService.BusinessId;
		var facilities = await _operatorIndustryService.GetOperatorsIndustries(operatorId!.Value);

		return facilities.ToList();
	}

	[HttpPost("industry")]
	[EnsureOperatorExists]
	[EnsureBusinessAccess(nameof(OperatorUser))]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult> AddOperatorIndustry([FromBody] OperatorIndustryCreationDto industryData)
	{
		var operatorId = _userPrincipalService.BusinessId;
		var existingOperator = await _operatorService.GetByIdAsync(operatorId!.Value);

		var newIndustry = _operatorIndustryService.MapIndustryToCreateOperator(existingOperator, industryData);

		await _operatorIndustryService.CreateAsync(newIndustry);

		return CreatedAtAction(nameof(GetOperatorFacility), new {operatorId = operatorId, industryId = newIndustry.Id }, newIndustry);
	}

	[HttpPut("industry/{industryId}")]
	[EnsureOperatorIndustryExists]
	[EnsureBusinessAccess(nameof(OperatorUser))]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult> UpdateOperatorIndustry(int industryId,
		UpdateOperatorIndustryDto industryData)
	{
		var operatorId = _userPrincipalService.BusinessId;
		var existingIndustry = await _operatorIndustryService.GetByIdAsync(operatorId!.Value, industryId);

		_operatorIndustryService.MapIndustryToUpdate(existingIndustry, industryData);

		await _operatorIndustryService.UpdateAsync(existingIndustry);
		return Ok(existingIndustry);
		
	}

	[HttpDelete("{operatorId}/industry/{industryId}")]
	[EnsureOperatorIndustryExists]
	[EnsureBusinessAccess(nameof(OperatorUser))]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult> RemoveOperatorIndustry(int industryId)
	{
		var operatorId = _userPrincipalService.BusinessId;
		var operatorIndustry = await _operatorIndustryService.GetByIdAsync(operatorId!.Value, industryId);

		await _operatorIndustryService.DeleteAsync(operatorIndustry);
		return NoContent();
	}
}
