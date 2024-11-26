using Product.Application.Dto;
using Product.Domain.Entity;

namespace Product.Application.ServiceInterfaces;

public interface IUserService
{
	Task CreateAsync(User user);
	Task<User> GetByIdAsync(int userId);
	Task UpdateAsync(User user);
	Task DeleteAsync(User user);
	Task<User> GetByEmailAsync(string email);
	void MapUserToUpdate(UserRegistrationByInviteDto dto, User user);
	UserDtoToFrontEnd MapUserToDto(User user);
	Task<string> Login(UserLoginDto userData);
}
