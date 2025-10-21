using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.Application.Dto;
using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Result;
using Product.Infrastructure.Filters;

namespace Product.WebApi.Controllers;

[Route("[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
	private readonly IUserService _userService;
	private readonly IInviteService _inviteService;
	private readonly IAuthService _authService;
	public AccountController(IInviteService inviteService, IUserService userService, IAuthService authService)
	{
		_inviteService = inviteService;
		_userService = userService;
		_authService = authService;
	}

	[HttpGet("Register/User/{inviteId}")]
	[EnsureInviteExists]
	public async Task<ActionResult<Response<InviteIdToFrontEnd>>> RegisterUser (int inviteId, CancellationToken cancellationToken)
	{
		var response = await _inviteService.RegisterUser(inviteId, cancellationToken);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}


	[HttpPost("Register/User/{inviteId}")]
	[EnsureInviteExists]
	public async Task<ActionResult<Response<UserDtoToFrontEnd>>> RegisterUserByInvite (int inviteId, 
		[FromBody] UserRegistrationByInviteDto registrationData, CancellationToken cancellationToken)
	{
		var response = await _inviteService.RegisterByInvite(inviteId, registrationData, cancellationToken);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpPost("Register/User")]
	public async Task<ActionResult<Response<UserDtoToFrontEnd>>> RegisterUser([FromBody] UserRegistrationDto registrationData,
		CancellationToken cancellationToken)
	{
		var response = await _authService.RegisterUser(registrationData, cancellationToken);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}


	[HttpGet("User/{userId}")]
	[EnsureUserExists]
	[Authorize(policy: "All")]
	public async Task<ActionResult<Response<UserDtoToFrontEnd>>> GetUser(int userId, CancellationToken cancellationToken)
	{
		var response = await _userService.GetUserAsync(userId, cancellationToken);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpPost("Login")]
	public async Task<ActionResult<Response<TokenDto>>> Login (UserLoginDto userData, CancellationToken cancellationToken)
	{
		var response = await _authService.Login(userData, cancellationToken);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpDelete("/{userId}")]
	[EnsureUserExists]
	[Authorize(policy: "Admin")]
	public async Task<ActionResult<Response<int>>> RemoveUser(int userId, CancellationToken cancellationToken)
	{
		var response = await _userService.RemoveUserAsync(userId, cancellationToken);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}
	
	[HttpPut("/User/{userId}")]
	[EnsureUserExists]
	[Authorize(policy: "All")]
	public async Task<ActionResult<Response<UserDtoToFrontEnd>>> UpdateUser(UserToUpdateDto userUpdateData, int userId,
		CancellationToken cancellationToken)
	{
		var response = await _userService.UpdateUserAsync(userUpdateData, userId, cancellationToken);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}

	[HttpPost("/refresh")]
	[Authorize(policy: "All")]
	public async Task<ActionResult<Response<TokenDto>>> RefreshToken([FromBody] RefreshTokenRequestDto tokenRequestDto,
		CancellationToken cancellationToken)
	{
		var response = await _userService.Refresh(tokenRequestDto, cancellationToken);
		if (response.IsSuccess)
		{
			return Ok(response);
		}
		return BadRequest(response);
	}
}
