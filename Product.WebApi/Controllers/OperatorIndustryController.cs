using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.Application.Dto;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Result;
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
	public async Task<ActionResult<Response<OpIndustryFrontEndDto>>> GetOperatorIndustry(int industryId)
	{
		var response = await _operatorIndustryService.GetOpIndustryByIdAsync(industryId);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpGet("industries")]
	[EnsureBusinessAccess(nameof(OperatorUser))]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<Response<List<OperatorIndustry>>>> GetOperatorIndustries()
	{
		var response = await _operatorIndustryService.GetOperatorsIndustriesAsync();
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpPost("industry")]
	[EnsureOperatorExists]
	[EnsureBusinessAccess(nameof(OperatorUser))]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<Response<OpIndustryFrontEndDto>>> AddOperatorIndustry([FromBody] OperatorIndustryCreationDto industryData)
	{
		var response = await _operatorIndustryService.CreateOperatorIndustryAsync(industryData);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpPut("industry/{industryId}")]
	[EnsureOperatorIndustryExists]
	[EnsureBusinessAccess(nameof(OperatorUser))]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<Response<OpIndustryFrontEndDto>>> UpdateOperatorIndustry(int industryId,
		UpdateOperatorIndustryDto industryData)
	{
		var response = await _operatorIndustryService.UpdateOperatorIndustryAsync(industryId, industryData);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpDelete("{operatorId}/industry/{industryId}")]
	[EnsureOperatorIndustryExists]
	[EnsureBusinessAccess(nameof(OperatorUser))]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<Response<int>>> RemoveOperatorIndustry(int industryId)
	{
		var response = await _operatorIndustryService.RemoveOperatorIndustryAsync(industryId);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}
}
