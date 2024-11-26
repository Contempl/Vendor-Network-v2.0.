using System.ComponentModel.DataAnnotations;

namespace Product.Application.Dto;

public class VendorRegistrationDto
{
	[Required]
	[MaxLength(76, ErrorMessage = "Business name shouldn't be longer than 76 symbols")]
	public string BusinessName { get; set; }

	[Required]
	[StringLength(75, ErrorMessage = "The adress shouldn't be longer than 75 symbols")]
	public string Adress { get; set; }

	[Required]
	public string Email { get; set; }
}
