using Product.Application.Dto;
using Product.Application.Interfaces;
using Product.Application.Mapping;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Enum;
using Product.Domain.Result;


namespace Product.Infrastructure.Implementations;

public class UserService : IUserService
{
	private readonly IUserRepository _userRepository;
	private readonly IUserPrincipalService _userPrincipalService;
	private readonly IPasswordHasher _passwordHasher;
	private readonly IJwtTokenService _jwtTokenService;
	private readonly IVendorUserRepository _vendorUserRepository;
	private readonly IOperatorUserRepository _operatorUserRepository;
	private readonly IRefreshTokenRepository _refreshTokenRepository;

	public UserService(IUserRepository userRepository, IPasswordHasher userPrincipalService, 
		IJwtTokenService jwtTokenService, IVendorUserRepository vendorUserRepository, 
		IOperatorUserRepository operatorUserRepository, 
		IUserPrincipalService userPrincipalService1, IRefreshTokenRepository refreshTokenRepository)
	{
		_userRepository = userRepository;
		_passwordHasher = userPrincipalService;
		_jwtTokenService = jwtTokenService;
		_vendorUserRepository = vendorUserRepository;
		_operatorUserRepository = operatorUserRepository;
		_userPrincipalService = userPrincipalService1;
		_refreshTokenRepository = refreshTokenRepository;
	}
	public void MapUserToUpdateByInvite(UserRegistrationByInviteDto dto, User user)
	{
		user.UserName = dto.UserName;
		user.FirstName = dto.FirstName;
		user.LastName = dto.LastName;
		user.PasswordHash = _passwordHasher.HashThePassword(dto.Password);
	}

	public async Task<Response<UserDtoToFrontEnd>> GetUserAsync(int userId, CancellationToken cancellationToken = default)
	{
		var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

		var userDto = user.MapToFrontEndDto();

		return new Response<UserDtoToFrontEnd>
		{
			Data = userDto,
		};
	}

	public async Task<Response<int>> RemoveUserAsync(int userId, CancellationToken cancellationToken = default)
	{
		var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
		
		await _userRepository.DeleteAsync(user, cancellationToken);
		
		return new Response<int>
		{
			Data = userId,
		};
	}

	public async Task<Response<UserDtoToFrontEnd>> UpdateUserAsync(UserToUpdateDto userUpdateData, int userId, 
		CancellationToken cancellationToken = default)
	{
		var thisUserId = _userPrincipalService.UserId!.Value;
		if (thisUserId != userId)
		{
			return new Response<UserDtoToFrontEnd>()
			{
				ErrorMessage = $"User {userId} does not match the current user",
				ErrorCode = (int)ErrorCodes.UsersDontMatch,
			};
		}
		
		var user = await _userRepository.GetByIdWithInvitesAsync(userId, cancellationToken);
		user.MapUserToUpdate(userUpdateData);
		
		await _userRepository.UpdateAsync(user, cancellationToken);
		return new Response<UserDtoToFrontEnd>
		{
			Data = user.MapToFrontEndDto()
		};
	}

	public async Task<Response<TokenDto>> Refresh(RefreshTokenRequestDto refreshDto, 
		CancellationToken cancellationToken = default)
	{
		var existingToken = await _refreshTokenRepository.GetByTokenAsync(refreshDto.RefreshToken, cancellationToken);

		if (existingToken == null || !_jwtTokenService.Validate(existingToken))
			return new Response<TokenDto>
			{
				ErrorMessage = "Refresh token expired",
				ErrorCode = (int)ErrorCodes.InvalidRefreshToken
			};
		
		await _refreshTokenRepository.RevokeAsync(existingToken, cancellationToken);
		
		var userId = existingToken.UserId;
		
		var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
		
		var userClaims = user.MapUserToClaimDto();

		var newToken = _jwtTokenService.GenerateToken(userClaims);
		
		var refreshToken = new RefreshToken
		{
			Token = newToken.RefreshToken,
			UserId = userId,
			ExpiresAt = DateTime.UtcNow.AddDays(7)
		};
		
		await _refreshTokenRepository.CreateAsync(refreshToken, cancellationToken);

		return new Response<TokenDto>
		{
			Data = newToken
		};
	}


	private VendorUser MapVendorUserFromDto(UserRegistrationDto registrationData) => new VendorUser
	{
		UserName = registrationData.UserName,
		FirstName = registrationData.FirstName,
		LastName = registrationData.LastName,
		Email = registrationData.Email,
		PasswordHash = _passwordHasher.HashThePassword(registrationData.Password),
		UserType = UserType.VendorUser,
	};

	private OperatorUser MapOperatorUserFromDto(UserRegistrationDto registrationData) => new OperatorUser
	{
		UserName = registrationData.UserName,
		FirstName = registrationData.FirstName,
		LastName = registrationData.LastName,
		Email = registrationData.Email,
		PasswordHash = _passwordHasher.HashThePassword(registrationData.Password),
		UserType = UserType.OperatorUser,
	};
}
