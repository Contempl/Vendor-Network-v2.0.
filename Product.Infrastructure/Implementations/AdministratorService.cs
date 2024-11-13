using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;
using Product.Infrastructure.Dto;

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
	public string GenerateEmailTemplate(string email, User user, string inviteUrl)
	{
		string emailTemplate = "";

		if (user is VendorUser)
			emailTemplate =
				$"<h1>Invitation to Register as a Vendor</h1>" +
				$"<p>Dear {email},</p>" +
				$"<p>You have been invited to register as a Vendor business. Please click the following link to complete your registration:</p>" +
				$"<a href=\"{inviteUrl}\">Register now</a>" +
				$"<p>This invitation will expire in 30 days.</p>";

		else if (user is OperatorUser)
			emailTemplate =
				$"<h1>Invitation to Register as an Operator business</h1>" +
				$"<p>Dear {email},</p>" +
				$"<p>You have been invited to register your business. Please click the following link to complete your registration:</p>" +
				$"<a href=\"{inviteUrl}\">Register now</a>" +
				$"<p>This invitation will expire in 30 days.</p>";

		else
			throw new ArgumentException($"Invalid User Type");

		return emailTemplate;
	}
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
