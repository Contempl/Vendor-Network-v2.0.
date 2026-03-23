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
public class AccountController : ControllerBase
{
	private readonly IUserService _userService;
	private readonly IInviteService _inviteService;
	private readonly IAuthService _authService;
	public AccountController(
		IInviteService inviteService, 
		IUserService userService, 
		IAuthService authService)
	{
		_inviteService = inviteService;
		_userService = userService;
		_authService = authService;
	}

	[HttpGet("Register/User/{inviteId}")]
	[EnsureInviteExists]
	public async Task<ActionResult<OneOf<InviteIdToFrontEnd, Error>>> GetInviteById (int inviteId, CancellationToken cancellationToken)
	{
		var response = await _inviteService.GetInviteById(inviteId, cancellationToken);
		
		return response.Match<ActionResult>(
			dto => Ok(dto),
			error => StatusCode(500, error));
	}


	[HttpPost("Register/User/{inviteId}")]
	[EnsureInviteExists]
	public async Task<ActionResult<OneOf<UserDtoToFrontEnd, ValidationError, NotFoundError, Error>>> RegisterUserByInvite (int inviteId, 
		[FromBody] UserRegistrationByInviteDto registrationData, CancellationToken cancellationToken)
	{
		var response = await _inviteService.RegisterByInvite(inviteId, registrationData, cancellationToken);
		
		return response.Match<ActionResult>(
			dto => Ok(dto),
			validationError => BadRequest(validationError),
			notFound => BadRequest(notFound),
			error => StatusCode(500, error));
	}

	[HttpPost("Register/User")]
	public async Task<ActionResult<OneOf<UserDtoToFrontEnd, NotFoundError, Error>>> RegisterUser([FromBody] UserRegistrationDto registrationData,
		CancellationToken cancellationToken)
	{
		var response = await _authService.RegisterUser(registrationData, cancellationToken);
		
		return response.Match<ActionResult>(
			dto => Ok(dto),
			notFound => BadRequest(notFound),
			error => StatusCode(500, error));
	}


	[HttpGet("User/{userId}")]
	[EnsureUserExists]
	[Authorize(policy: "All")]
	public async Task<ActionResult<OneOf<UserDtoToFrontEnd, Error>>> GetUser(int userId, CancellationToken cancellationToken)
	{
		var response = await _userService.GetUserAsync(userId, cancellationToken);

		return response.Match<ActionResult>(
			userDto => Ok(userDto),
			error => StatusCode(500, error));
	}

	[HttpPost("Login")]
	public async Task<ActionResult<OneOf<TokenDto, NotFoundError, ValidationError, Error>>> Login (UserLoginDto userData, CancellationToken cancellationToken)
	{
		var response = await _authService.Login(userData, cancellationToken);
		
		return response.Match<ActionResult>(
			dto => Ok(dto),
			notFound => BadRequest(notFound),
			validationError => BadRequest(validationError),
			error => StatusCode(500, error));
	}

	[HttpDelete("/{userId}")]
	[EnsureUserExists]
	[Authorize(policy: "Admin")]
	public async Task<ActionResult<OneOf<int, Error>>> RemoveUser(int userId, CancellationToken cancellationToken)
	{
		var response = await _userService.RemoveUserAsync(userId, cancellationToken);
		
		return response.Match<ActionResult>(
			id => Ok(id),
			error => StatusCode(500, error));
	}
	
	[HttpPut("/User/{userId}")]
	[EnsureUserExists]
	[Authorize(policy: "All")]
	public async Task<ActionResult<OneOf<UserDtoToFrontEnd, ValidationError, Error>>> UpdateUser(UserToUpdateDto userUpdateData, int userId,
		CancellationToken cancellationToken)
	{
		var response = await _userService.UpdateUserAsync(userUpdateData, userId, cancellationToken);
		
		return response.Match<ActionResult>(
			dto => Ok(dto),
			validationError => BadRequest(validationError),
			error => StatusCode(500, error));
	}

	[HttpPost("/refresh")]
	[Authorize(policy: "All")]
	public async Task<ActionResult<OneOf<TokenDto, ValidationError, Error>>> RefreshToken([FromBody] RefreshTokenRequestDto tokenRequestDto,
		CancellationToken cancellationToken)
	{
		var response = await _authService.Refresh(tokenRequestDto, cancellationToken);
		
		return response.Match<ActionResult>(
			dto => Ok(dto),
			validationError => BadRequest(validationError),
			error => StatusCode(500, error));
	}
}
