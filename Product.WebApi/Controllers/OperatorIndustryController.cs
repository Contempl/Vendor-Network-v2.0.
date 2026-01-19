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
	public async Task<ActionResult<OneOf<OpIndustryFrontEndDto, Error>>> GetOperatorIndustry(int industryId, CancellationToken cancellationToken)
	{
		var response = await _operatorIndustryService.GetOpIndustryByIdAsync(industryId, cancellationToken);

		return response.Match<ActionResult>(
			dto => Ok(response),
			error => StatusCode(500, error));
	}

	[HttpGet("industries")]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<OneOf<List<OperatorIndustry>, Error>>> GetOperatorIndustries(CancellationToken cancellationToken)
	{
		var response = await _operatorIndustryService.GetOperatorsIndustriesAsync(cancellationToken);
		
		return response.Match<ActionResult>(
			industryList => Ok(industryList),
			error => StatusCode(500, error));
	}

	[HttpPost("{operatorId}/industry")]

	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<OneOf<OpIndustryFrontEndDto, Error>>> AddOperatorIndustry(
		[FromBody] OperatorIndustryCreationDto industryData, CancellationToken cancellationToken)
	{
		var response = await _operatorIndustryService.CreateOperatorIndustryAsync(industryData, cancellationToken);
		
		return response.Match<ActionResult>(
			dto => Ok(dto),
			error => StatusCode(500, error));
	}

	[HttpPut("industry/{industryId}")]
	[EnsureOperatorIndustryExists]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<OneOf<OpIndustryFrontEndDto, NotFoundError, Error>>> UpdateOperatorIndustry(int industryId,
		UpdateOperatorIndustryDto industryData, CancellationToken cancellationToken)
	{
		var response = await _operatorIndustryService.UpdateOperatorIndustryAsync(industryId, industryData, cancellationToken);
		
		return response.Match<ActionResult>(
			dto => Ok(dto),
			notFoundError =>  BadRequest(notFoundError),
			error => StatusCode(500, error));
	}

	[HttpDelete("industry/{industryId}")]
	[EnsureOperatorIndustryExists]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<OneOf<int, Error>>> RemoveOperatorIndustry(int industryId, 
		CancellationToken cancellationToken)
	{
		var response = await _operatorIndustryService.RemoveOperatorIndustryAsync(industryId, cancellationToken);
		
		return response.Match<ActionResult>(
			result => Ok(result),
			error => StatusCode(500, error));
	}
}
