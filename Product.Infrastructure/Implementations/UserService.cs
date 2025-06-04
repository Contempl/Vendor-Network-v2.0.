using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
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
	private readonly IRedisCacheService _redisCacheService;

	public UserService(IUserRepository userRepository, IPasswordHasher userPrincipalService, IJwtTokenService jwtTokenService, IVendorUserRepository vendorUserRepository, IOperatorUserRepository operatorUserRepository, IRedisCacheService redisCacheService, IUserPrincipalService userPrincipalService1)
	{
		_userRepository = userRepository;
		_passwordHasher = userPrincipalService;
		_jwtTokenService = jwtTokenService;
		_vendorUserRepository = vendorUserRepository;
		_operatorUserRepository = operatorUserRepository;
		_redisCacheService = redisCacheService;
		_userPrincipalService = userPrincipalService1;
	}
	public void MapUserToUpdateByInvite(UserRegistrationByInviteDto dto, User user)
	{
		user.UserName = dto.UserName;
		user.FirstName = dto.FirstName;
		user.LastName = dto.LastName;
		user.PasswordHash = _passwordHasher.HashThePassword(dto.Password);
	}

	public async Task<Response<UserDtoToFrontEnd>> RegisterUser(UserRegistrationDto registrationData)
	{
		var userWithThisEmail = await _userRepository.GetByEmailAsync(registrationData.Email);
		if (userWithThisEmail != null)
		{
			return new Response<UserDtoToFrontEnd>
			{
				ErrorMessage = "User with this email already exists!",
				ErrorCode = (int)ErrorCodes.UserWithThisEmailAlreadyExists
			};
		}
		
		if (!registrationData.IsOperator)
		{
			var newVendor = MapVendorUserFromDto(registrationData);

			await _vendorUserRepository.CreateAsync(newVendor);
			var vendorUserDto = newVendor.MapToFrontEndDto();
			return new Response<UserDtoToFrontEnd>
			{
				Data = vendorUserDto
			};
		}
		
		var newOperator = MapOperatorUserFromDto(registrationData);
		await _operatorUserRepository.CreateAsync(newOperator);
		
		var operatorUserDto = newOperator.MapToFrontEndDto();

		return new Response<UserDtoToFrontEnd>
		{
			Data = operatorUserDto
		};
	}
	public async Task<Response<UserDtoToFrontEnd>> GetUserAsync(int userId)
	{
		var cacheKey = $"User_{userId}";
		var cachedUser = await _redisCacheService.GetAsync<UserDtoToFrontEnd>(cacheKey);

		if (cachedUser is not null)
		{
			return new Response<UserDtoToFrontEnd>
			{
				Data = cachedUser
			};
		}
		
		var existingUser = await _userRepository.GetByIdAsync(userId);
		if (existingUser == null)
		{
			return new Response<UserDtoToFrontEnd>
			{
				ErrorMessage = "User was not found in the database.",
				ErrorCode = (int)ErrorCodes.UserNotFound,
			};
		}
		var userDto = existingUser.MapToFrontEndDto();

		return new Response<UserDtoToFrontEnd>
		{
			Data = userDto,
		};
	}

	public async Task<Response<TokenDto>> Login(UserLoginDto userData)
	{
		var user = await _userRepository.GetByEmailAsync(userData.Email);

		if (user == null)
		{
			return new Response<TokenDto>
			{
				ErrorMessage = "User not found",
				ErrorCode = (int)ErrorCodes.UserNotFound
			};
		}

		var passwordsAreEqual = _passwordHasher.ValidatePassword(userData.Password, user.PasswordHash!);

		if (!passwordsAreEqual)
		{
			return new Response<TokenDto>
			{
				ErrorMessage = "Invalid password",
				ErrorCode = (int)ErrorCodes.InvalidPassword
			};
		}

		var token = _jwtTokenService.GenerateToken(user);

		return new Response<TokenDto>
		{
			Data = token,
		};
	}

	public async Task<Response<int>> RemoveUserAsync(int userId)
	{
		var user = await _userRepository.GetByIdAsync(userId);
		await _userRepository.DeleteAsync(user);
		
		return new Response<int>
		{
			Data = userId,
		};
	}

	public async Task<Response<UserDtoToFrontEnd>> UpdateUserAsync(UserToUpdateDto userUpdateData, int userId)
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
		
		var user = await _userRepository.GetByIdWithInvitesAsync(userId);
		MapUserToUpdate(user, userUpdateData);
		
		await _userRepository.UpdateAsync(user);
		return new Response<UserDtoToFrontEnd>
		{
			Data = user.MapToFrontEndDto()
		};
	}

	private void MapUserToUpdate(User user, UserToUpdateDto userUpdateData)
	{
		user.UserName = userUpdateData.UserName ?? user.UserName;
		user.FirstName = userUpdateData.FirstName ?? user.FirstName;
		user.LastName = userUpdateData.LastName ?? user.LastName;
		user.Email = userUpdateData.Email ?? user.Email;
	}

	public async Task<Response<UserDto>> AddUserToCache(long userId)
	{
		var user = await _userRepository.GetAll().FirstOrDefaultAsync(x => x.Id == userId);
		if (user == null)
		{
			return new Response<UserDto>()
			{
				ErrorMessage = "Пользователь не найден",
				ErrorCode = (int)ErrorCodes.UserNotFound,
			};
		}
        
		var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(1));

		#region MemoryCache

		//var options = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(2));
		// _memoryCache.Set($"User_{user.Id}", user, options);

		#endregion
        
		await _redisCacheService.SetAsync($"User_{user.Id}", user, options);
		Debug.Write($"В кеш добавился ключ User_{user.Id}");

		return new Response<UserDto>()
		{
			Data = user.MapToDto()
		};
	}
	
	public async Task<Response<UserDto>> TryGetUserFromCache(long userId)
	{
		var cacheKey = $"User_{userId}";
		var cachedUser = await _redisCacheService.GetAsync<User>(cacheKey);

		if (cachedUser != null)
		{
			Debug.WriteLine($"User found in cache: {cacheKey}");
			return new Response<UserDto>
			{
				Data = cachedUser.MapToDto()
			};
		}
		Debug.WriteLine($"Cache miss for: {cacheKey}");


		var user = await _userRepository.GetAll().FirstOrDefaultAsync(x => x.Id == userId);
		if (user == null)
		{
			return new Response<UserDto>
			{
				ErrorMessage = "Пользователь не найден"
			};
		}

		await _redisCacheService.SetAsync(cacheKey, user);

		return new Response<UserDto>
		{
			Data = user.MapToDto()
		};
	}
	

	private VendorUser MapVendorUserFromDto(UserRegistrationDto registrationData) => new VendorUser
	{
		UserName = registrationData.UserName,
		FirstName = registrationData.FirstName,
		LastName = registrationData.LastName,
		Email = registrationData.Email,
		PasswordHash = _passwordHasher.HashThePassword(registrationData.Password),
	};

	private OperatorUser MapOperatorUserFromDto(UserRegistrationDto registrationData) => new OperatorUser
	{
		UserName = registrationData.UserName,
		FirstName = registrationData.FirstName,
		LastName = registrationData.LastName,
		Email = registrationData.Email,
		PasswordHash = _passwordHasher.HashThePassword(registrationData.Password)
	};
}
