using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Types;
using Product.Application.Dto;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Result;
using Product.Infrastructure.Filters;

namespace Product.WebApi.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize(policy: "Admin")]
public class AdminController : ControllerBase
{
	private readonly IAdministratorService _adminService;
	private readonly IAuthService _authService;

	public AdminController(IAdministratorService adminService, IAuthService authService)
	{
		_adminService = adminService;
		_authService = authService;
	}
	
	[AllowAnonymous]
	[HttpPost("Login")]
	public async Task<ActionResult<OneOf<TokenDto, NotFoundError, ValidationError, Error>>> Login (UserLoginDto userData, CancellationToken cancellationToken)
	{
		var response = await _authService.LoginAdministrator(userData, cancellationToken);
		
		return response.Match<ActionResult>(
			dto => Ok(dto),
			notFound => BadRequest(notFound),
			validationError => BadRequest(validationError),
			error => StatusCode(500, error));
	}

	[HttpPost("inviteBusiness")]
	public async Task<ActionResult<OneOf<UserDtoToFrontEnd, ValidationError, Error>>> InviteBusiness([FromBody] BusinessInvitationData invitationData,
		CancellationToken cancellationToken)
	{
		var response = await _adminService.InviteBusiness(invitationData, cancellationToken);

		return response.Match<ActionResult>(
			userDto => Ok(userDto),
			validationError => BadRequest(validationError),
			error => StatusCode(500, error)
		);
	}

	[HttpPost("/inviteVendorUser")]
	public async Task<ActionResult<OneOf<UserDtoToFrontEnd, ValidationError, Error>>> InviteVendorUser([FromBody] DataForInviteDto inviteData,
		CancellationToken cancellationToken)
	{
		var response = await _adminService.InviteVendorUser(inviteData, cancellationToken);
		
		return response.Match<ActionResult>(
			userDto => Ok(userDto),
			validationError => BadRequest(validationError), 
			error => StatusCode(500, error)
		);
	}

	[HttpPost("/inviteOperatorUser")]
	public async Task<ActionResult<OneOf<UserDtoToFrontEnd, ValidationError, Error>>> InviteOperatorUser([FromBody] DataForInviteDto inviteData, CancellationToken cancellationToken)
	{
		var response = await _adminService.InviteOperatorUser(inviteData, cancellationToken);
		
		return response.Match<ActionResult>(
			userDto => Ok(userDto),
			validationError => BadRequest(validationError),
			error => StatusCode(500, error)
		);
	}
	
	
	[HttpDelete("Vendor/{vendorId}")]
	[EnsureVendorExists]
	[Authorize(policy: "Admin")]
	public async Task<ActionResult<OneOf<int, Error>>> RemoveVendor(int vendorId, CancellationToken cancellationToken)
	{
		var response = await _adminService.RemoveVendorAsync(vendorId, cancellationToken);
		
		return response.Match<ActionResult>(
			id => Ok(id),
			error => StatusCode(500, error)
		);
	} 
	
	[HttpDelete("Operator/{operatorId}")]
	[EnsureOperatorExists]
	[Authorize(policy: "Admin")]
	public async Task<ActionResult<OneOf<int, Error>>> RemoveOperator(int operatorId, CancellationToken cancellationToken)
	{
		var response = await _adminService.RemoveOperatorAsync(operatorId, cancellationToken);
		
		return response.Match<ActionResult>(
			id => Ok(id),
			error => StatusCode(500, error)
		);
	}
}
