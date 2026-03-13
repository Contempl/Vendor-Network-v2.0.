using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using Product.Application.Interfaces;
using Product.Application.Mapping;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Result;


namespace Product.Infrastructure.Implementations;

public class UserService : IUserService
{
	private readonly IUserRepository _userRepository;
	private readonly IUserPrincipalService _userPrincipalService;
	private readonly ILogger<UserService> _logger;

	public UserService(IUserRepository userRepository, IUserPrincipalService userPrincipalService1, ILogger<UserService> logger)
	{
		_userRepository = userRepository;
		_userPrincipalService = userPrincipalService1;
		_logger = logger;
	}

	public async Task<OneOf<UserDtoToFrontEnd, Error>> GetUserAsync(int userId, CancellationToken cancellationToken)
	{
		try
		{
			var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

			var userDto = user.MapToFrontEndDto();

			return userDto;
		}
		catch (KeyNotFoundException ex)
		{
			_logger.LogWarning(ex, "User with given Id was not found.");
			throw;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting user.");
			return new Error();
		}
	}

	public async Task<OneOf<int, Error>> RemoveUserAsync(int userId, CancellationToken cancellationToken = default)
	{
		try
		{
			var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
		
			await _userRepository.DeleteAsync(user, cancellationToken);

			return userId;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting user.");
			return new Error();
		}
	}

	public async Task<OneOf<UserDtoToFrontEnd, ValidationError, Error>> UpdateUserAsync(UserToUpdateDto userUpdateData, int userId, 
		CancellationToken cancellationToken = default)
	{
		try
		{
			var thisUserId = _userPrincipalService.UserId!.Value;
			if (thisUserId != userId)
			{
				_logger.LogWarning("Cannot update user because userId does not match.");
				return new ValidationError();
			}
		
			var user = await _userRepository.GetByIdWithInvitesAsync(userId, cancellationToken);
			user.MapUserToUpdate(userUpdateData);
		
			await _userRepository.UpdateAsync(user, cancellationToken);
			var result = user.MapToFrontEndDto();

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating user.");
			return new Error();
		}
	}
}
