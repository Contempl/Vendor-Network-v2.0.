using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Product.Infrastructure.Dto;

public class UserRegistrationByInviteDto
{
	public int InviteId { get; set; }

	[Required]
	public string UserName { get; set; }

	[Required]
	public string FirstName { get; set; }

	[Required]
	public string LastName { get; set; }

	[Required]
	[StringLength(50, ErrorMessage = "The password must be at least 6 and at max 50 characters long.",
		MinimumLength = 6)]
	[PasswordPropertyText]
	public string Password { get; set; }
}
