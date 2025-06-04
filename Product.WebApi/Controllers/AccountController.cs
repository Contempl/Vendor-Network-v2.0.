using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.Application.Dto;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Result;
using Product.Infrastructure.Filters;

namespace Product.WebApi.Controllers;

[Route("[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
	private readonly IUserService _userService;
	private readonly IInviteService _inviteService;

	public AccountController(IInviteService inviteService, IUserService userService, IAdministratorService adminService)
	{
		_inviteService = inviteService;
		_userService = userService;
	}

	[HttpGet("Register/User/{inviteId}")]
	[EnsureInviteExists]
	public async Task<ActionResult<Response<InviteIdToFrontEnd>>> RegisterUser (int inviteId)
	{
		var response = await _inviteService.Register(inviteId);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}


	[HttpPost("Register/User/{inviteId}")]
	[EnsureInviteExists]
	public async Task<ActionResult<Response<UserDtoToFrontEnd>>> RegisterUserByInvite (int inviteId, 
		[FromBody] UserRegistrationByInviteDto registrationData)
	{
		var response = await _inviteService.RegisterByInvite(inviteId, registrationData);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpPost("Register/User")]
	public async Task<ActionResult<Response<UserDtoToFrontEnd>>> RegisterUser([FromBody] UserRegistrationDto registrationData)
	{
		var response = await _userService.RegisterUser(registrationData);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}


	[HttpGet("User/{userId}")]
	[EnsureUserExists]
	[Authorize(policy: "All")]
	public async Task<ActionResult<Response<UserDtoToFrontEnd>>> GetUser(int userId)
	{
		var response = await _userService.GetUserAsync(userId);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpPost("Login")]
	public async Task<ActionResult<Response<TokenDto>>> Login (UserLoginDto userData)
	{
		var response = await _userService.Login(userData);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpDelete("/{userId}")]
	[EnsureUserExists]
	[Authorize(policy: "AdminOnly")]
	public async Task<ActionResult<Response<int>>> RemoveUser(int userId)
	{
		var response = await _userService.RemoveUserAsync(userId);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}
	
	[HttpPut("/User/{userId}")]
	[EnsureUserExists]
	[Authorize(policy: "All")]
	public async Task<ActionResult<Response<UserDtoToFrontEnd>>> UpdateUser(UserToUpdateDto userUpdateData, int userId)
	{
		var response = await _userService.UpdateUserAsync(userUpdateData, userId);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}
}
