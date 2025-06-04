using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.Application.Dto;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Pagination;
using Product.Domain.Result;
using Product.Infrastructure.Filters;

namespace Product.WebApi.Controllers;

[Route("[controller]")]
[ApiController]
public class OperatorController : Controller
{
	private readonly IOperatorService _operatorService;
	private readonly IUserPrincipalService _userPrincipalService;

	public OperatorController(IOperatorService operatorService, IUserPrincipalService userPrincipalService)
	{
		_operatorService = operatorService;
		_userPrincipalService = userPrincipalService;
	}

	[HttpPost("search/vendors")]
	[EnsureBusinessAccess(nameof(OperatorUser))]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<Response<List<BusinessFrontEndDto>>>> SearchVendorsToServeFacilities([FromBody] SearchVendorsForIndustriesDto industriesData)
	{
		var response = await _operatorService.SearchForVendorsAsync(industriesData);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpPost("search/vendor")]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<Response<PagedList<Vendor>>>> GetVendors([FromBody] VendorSearchDto vendorSearchDto)
	{
		var response =  await _operatorService.GetVendorsByNameAsync(vendorSearchDto);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}


	[HttpGet]
	[EnsureBusinessAccess(nameof(OperatorUser))]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<Response<BusinessFrontEndDto>>> GetOperator()
	{
		var operatorId = _userPrincipalService.BusinessId!.Value;
		var response = await _operatorService.GetOperatorAsync(operatorId);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpPost("register/{operatorUserId}")]
	[EnsureOperatorUserExists]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<Operator>> RegisterOperator(int operatorUserId, [FromBody] OperatorRegistrationDto operatorRegistrationData)
	{
		var response = await _operatorService.RegisterOperatorAsync(operatorUserId, operatorRegistrationData);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpPut]
	[EnsureBusinessAccess(nameof(OperatorUser))]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<Response<BusinessFrontEndDto>>> UpdateOperator([FromBody] UpdateOperatorDto operatorData)
	{
		var operatorId = _userPrincipalService.BusinessId!.Value;
		var response = await _operatorService.UpdateOperatorAsync(operatorId, operatorData);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpDelete("{operatorId}")]
	[EnsureOperatorExists]
	[Authorize(policy: "AdminOnly")]
	public async Task<ActionResult<Response<int>>> DeleteOperator(int operatorId)
	{
		var response = await _operatorService.DeleteOperatorAsync(operatorId);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpPost("invite")]
	[EnsureBusinessAccess(nameof(OperatorUser))]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<Response<MailMsg>>> InviteOperatorUser([FromBody] EmailForInviteDto dto)
	{
		var operatorUserId = _userPrincipalService.UserId!.Value;
		var response = await _operatorService.InviteOperatorUserAsync(operatorUserId, dto);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}
}
