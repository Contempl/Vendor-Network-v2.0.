using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;
using Product.Infrastructure.Dto;

namespace Product.Infrastructure.Implementations;

public class UserService : IUserService
{
	private readonly IUserRepository _userRepository;
	private readonly IPasswordHasher _passwordHasher;
	private readonly IJwtTokenService _jwtTokenService;

	public UserService(IUserRepository userRepository, IPasswordHasher userPrincipalService, IJwtTokenService jwtTokenService)
	{
		_userRepository = userRepository;
		_passwordHasher = userPrincipalService;
		_jwtTokenService = jwtTokenService;
	}

	public Task CreateAsync(User user) => _userRepository.CreateAsync(user);
	public Task DeleteAsync(User user) => _userRepository.DeleteAsync(user);
	public Task<User> GetByIdAsync(int userId) => _userRepository.GetByIdAsync(userId);
	public Task UpdateAsync(User user) => _userRepository.UpdateAsync(user);
	public Task<User> GetByEmailAsync(string email) => _userRepository.GetByEmailAsync(email);
	public void MapUserToUpdate(UserRegistrationByInviteDto dto, User user)
	{
		user.UserName = dto.UserName;
		user.FirstName = dto.FirstName;
		user.LastName = dto.LastName;
		user.PasswordHash = _passwordHasher.HashThePassword(dto.Password);
	}

	public UserDtoToFrontEnd MapUserToDto(User user)
	{
		var dto = new UserDtoToFrontEnd
		{
			Id = user.Id,
			UserName = user.UserName,
			Email = user.Email,
			FirstName = user.FirstName,
			LastName = user.LastName,
		};
		return dto;
	}

	public async Task<string> Login(UserLoginDto userData)
	{
		var user = await _userRepository.GetByEmailAsync(userData.UserName);

		if (user == null)
		{
			return string.Empty;
		}

		var passwordsAreEqual = _passwordHasher.ValidatePassword(userData.Password, user.PasswordHash!);

		if (!passwordsAreEqual)
		{
			return string.Empty;
		}

		var token = _jwtTokenService.GenerateToken(user);

		return token;
	}
}
