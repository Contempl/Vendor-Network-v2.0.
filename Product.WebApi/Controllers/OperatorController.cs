using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Types;
using Product.Application.Dto;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Enum;
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
	[EnsureBusinessAccess(UserType.OperatorUser)]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<OneOf<List<BusinessFrontEndDto>, ValidationError, Error>>> SearchVendorsToServeFacilities(
		[FromBody] SearchVendorsForIndustriesDto industriesData, CancellationToken cancellationToken)
	{
		var response = await _operatorService.SearchForVendorsAsync(industriesData, cancellationToken);
		
		return response.Match<ActionResult>(
			vendors => Ok(vendors),
			notFound => NotFound(notFound),
			validationError => BadRequest(validationError),
			error => StatusCode(500, error));
	}

	[HttpPost("search/vendor")]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<OneOf<PagedList<Vendor>, Error>>> GetVendors(
		[FromBody] VendorSearchDto vendorSearchDto, CancellationToken cancellationToken)
	{
		var response =  await _operatorService.GetVendorsByNameAsync(vendorSearchDto, cancellationToken);
		
		return response.Match<ActionResult>(
			vendors => Ok(vendors),
			error => StatusCode(500, error));
	}


	[HttpGet]
	[EnsureBusinessAccess(UserType.OperatorUser)]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<OneOf<BusinessFrontEndDto, Error>>> GetOperator(CancellationToken cancellationToken)
	{
		var operatorId = _userPrincipalService.BusinessId!.Value;
		var response = await _operatorService.GetOperatorAsync(operatorId, cancellationToken);

		return response.Match<ActionResult>(
			@operator => Ok(@operator),
			error => StatusCode(500, error));
	}

	[HttpPut]
	[EnsureBusinessAccess(UserType.OperatorUser)]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<OneOf<BusinessFrontEndDto, Error>>> UpdateOperator(
		[FromBody] UpdateOperatorDto operatorData, CancellationToken cancellationToken)
	{
		var response = await _operatorService.UpdateOperatorAsync(operatorData, cancellationToken);
		
		return response.Match<ActionResult>(
			@operator => Ok(@operator),
			error => StatusCode(500, error));
	}

	[HttpPost("invite")]
	[EnsureBusinessAccess(UserType.OperatorUser)]
	[Authorize(policy: "OperatorUser")]
	public async Task<ActionResult<OneOf<MailMsg, Error>>> InviteOperatorUser(
		[FromBody] EmailForInviteDto dto, CancellationToken cancellationToken)
	{
		var response = await _operatorService.InviteOperatorUserAsync(dto, cancellationToken);
		
		return response.Match<ActionResult>(
			mailMsg => Ok(mailMsg),
			error => StatusCode(500, error));
	}
}
