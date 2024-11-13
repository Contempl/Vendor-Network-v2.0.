using System.ComponentModel.DataAnnotations;

namespace Product.Infrastructure.Dto;

public class UpdateVendorDto
{
	[MaxLength(76, ErrorMessage = "Business name shouldn't be longer than 76 symbols")]
	public string? BusinessName { get; set; }

	[MaxLength(75, ErrorMessage = "The adress shouldn't be longer than 75 symbols")]
	public string? Address { get; set; }

	public string? Email { get; set; }
}
