using Product.Application.Dto;
using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;

namespace Product.Infrastructure.Implementations;

public class AdministratorService : IAdministratorService
{
    private readonly IAdministratorRepository _adminRepository;

    public AdministratorService(IAdministratorRepository administratorRepository)
    {
        _adminRepository = administratorRepository;
    }

	public async Task CreateAsync(Administrator admin) => await _adminRepository.CreateAsync(admin);
	public async Task DeleteAsync(Administrator admin) => await _adminRepository.DeleteAsync(admin);
	public async Task<Administrator> GetByIdAsync(int adminId) => await _adminRepository.GetByIdAsync(adminId);
	public async Task UpdateAsync(Administrator admin) => await _adminRepository.UpdateAsync(admin);
	public Administrator MapAdminFromDto(AdminRegistrationDto registrationDto) => new Administrator
	{
		UserName = registrationDto.UserName,
		FirstName = registrationDto.FirstName,
		LastName = registrationDto.LastName,
		Email = registrationDto.Email,
		PasswordHash = HashThePassword(registrationDto.Password)
	};
	private byte[] HashThePassword(string password)
	{
		using (var hmac = new System.Security.Cryptography.HMACSHA512())
		{
			var passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
			return passwordHash;
		}
	}
}
