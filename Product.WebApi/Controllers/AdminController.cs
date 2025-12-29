using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
	private readonly IUserPrincipalService _userPrincipalService;
	private readonly IAuthService _authService;

	public AdminController(IAdministratorService adminService, IUserPrincipalService userPrincipalService, IAuthService authService)
	{
		_adminService = adminService;
		_userPrincipalService = userPrincipalService;
		_authService = authService;
	}
	
	[AllowAnonymous]
	[HttpPost("Login")]
	public async Task<ActionResult<Response<TokenDto>>> Login (UserLoginDto userData, CancellationToken cancellationToken)
	{
		var response = await _authService.LoginAdministrator(userData, cancellationToken);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpPost("inviteBusiness")]
	public async Task<ActionResult<Response<UserDtoToFrontEnd>>> InviteBusiness([FromBody] BusinessInvitationData invitationData,
		CancellationToken cancellationToken)
	{
		var response = await _adminService.InviteBusiness(invitationData, cancellationToken);

		return response.Match<ActionResult>(
			userDto => Ok(userDto),
			error => BadRequest(error)
		);
	}

	[HttpPost("/inviteVendorUser")]
	public async Task<ActionResult<Response<UserDtoToFrontEnd>>> InviteVendorUser([FromBody] DataForInviteDto inviteData,
		CancellationToken cancellationToken)
	{
		var response = await _adminService.InviteVendorUser(inviteData, cancellationToken);
		
		return response.Match<ActionResult>(
			userDto => Ok(userDto),
			error => BadRequest(error)
		);
	}

	[HttpPost("/inviteOperatorUser")]
	public async Task<ActionResult<Response<UserDtoToFrontEnd>>> InviteOperatorUser([FromBody] DataForInviteDto inviteData, CancellationToken cancellationToken)
	{
		var response = await _adminService.InviteOperatorUser(inviteData, cancellationToken);
		
		return response.Match<ActionResult>(
			userDto => Ok(userDto),
			error => BadRequest(error)
		);
	}
	
	
	[HttpDelete("Vendor/{vendorId}")]
	[EnsureVendorExists]
	[Authorize(policy: "Admin")]
	public async Task<ActionResult<Response<int>>> RemoveVendor(int vendorId, CancellationToken cancellationToken)
	{
		var response = await _adminService.RemoveVendorAsync(vendorId, cancellationToken);
		
		return response.Match<ActionResult>(
			id => Ok(id),
			error => BadRequest(error)
		);
	} 
	
	[HttpDelete("Operator/{operatorId}")]
	[EnsureOperatorExists]
	[Authorize(policy: "Admin")]
	public async Task<ActionResult<Response<int>>> RemoveOperator(int operatorId, CancellationToken cancellationToken)
	{
		var response = await _adminService.RemoveOperatorAsync(operatorId, cancellationToken);
		
		return response.Match<ActionResult>(
			id => Ok(id),
			error => BadRequest(error)
		);
	}
}
