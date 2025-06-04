using Product.Application.Dto;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Result;

namespace Product.Application.ServiceInterfaces;

public interface IUserService
{
	Task<Response<UserDtoToFrontEnd>> GetUserAsync(int userId);
	void MapUserToUpdateByInvite(UserRegistrationByInviteDto dto, User user);
	Task<Response<UserDtoToFrontEnd>> RegisterUser(UserRegistrationDto registrationData);
	Task<Response<TokenDto>> Login(UserLoginDto userData);
	Task<Response<int>> RemoveUserAsync(int userId);
	Task<Response<UserDtoToFrontEnd>> UpdateUserAsync(UserToUpdateDto userUpdateData, int userId);
	Task<Response<UserDto>> AddUserToCache(long userId);
}
