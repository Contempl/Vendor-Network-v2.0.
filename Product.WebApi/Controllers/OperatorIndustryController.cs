using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.Application.Dto;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Enum;
using Product.Domain.Result;
using Product.Infrastructure.Filters;

namespace Product.WebApi.Controllers;

[Route("operator")]
[ApiController]
[EnsureBusinessAccess(UserType.OperatorUser)]
public class OperatorIndustryController : ControllerBase
{
	private readonly IOperatorIndustryService _operatorIndustryService;


	public OperatorIndustryController(IOperatorIndustryService operatorIndustryService)
	{
		_operatorIndustryService = operatorIndustryService;
	}

	[HttpGet("industry/{industryId}")]
	[EnsureOperatorIndustryExists]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<Response<OpIndustryFrontEndDto>>> GetOperatorIndustry(int industryId, CancellationToken cancellationToken)
	{
		var response = await _operatorIndustryService.GetOpIndustryByIdAsync(industryId, cancellationToken);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpGet("industries")]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<Response<List<OperatorIndustry>>>> GetOperatorIndustries(CancellationToken cancellationToken)
	{
		var response = await _operatorIndustryService.GetOperatorsIndustriesAsync(cancellationToken);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpPost("{operatorId}/industry")]

	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<Response<OpIndustryFrontEndDto>>> AddOperatorIndustry([FromBody] OperatorIndustryCreationDto industryData,
		CancellationToken cancellationToken)
	{
		var response = await _operatorIndustryService.CreateOperatorIndustryAsync(industryData, cancellationToken);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpPut("industry/{industryId}")]
	[EnsureOperatorIndustryExists]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<Response<OpIndustryFrontEndDto>>> UpdateOperatorIndustry(int industryId,
		UpdateOperatorIndustryDto industryData, CancellationToken cancellationToken)
	{
		var response = await _operatorIndustryService.UpdateOperatorIndustryAsync(industryId, industryData, cancellationToken);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpDelete("industry/{industryId}")]
	[EnsureOperatorIndustryExists]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<Response<int>>> RemoveOperatorIndustry(int industryId, CancellationToken cancellationToken)
	{
		var response = await _operatorIndustryService.RemoveOperatorIndustryAsync(industryId, cancellationToken);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}
}
