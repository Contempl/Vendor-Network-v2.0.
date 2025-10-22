using Product.Application.Interfaces;
using Product.Application.Mapping;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Enum;
using Product.Domain.Result;


namespace Product.Infrastructure.Implementations;

public class UserService : IUserService
{
	private readonly IUserRepository _userRepository;
	private readonly IUserPrincipalService _userPrincipalService;

	public UserService(IUserRepository userRepository, IUserPrincipalService userPrincipalService1)
	{
		_userRepository = userRepository;
		_userPrincipalService = userPrincipalService1;
	}

	public async Task<Response<UserDtoToFrontEnd>> GetUserAsync(int userId, CancellationToken cancellationToken)
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
}
