using System.ComponentModel.DataAnnotations;

namespace Product.Infrastructure.Dto;

public class UpdateOperatorDto
{
	[StringLength(100, ErrorMessage = "The name of a company is too long")]
	public string? BusinessName { get; set; }

	[StringLength(100, ErrorMessage = "The adress is too long")]
	public string? Address { get; set; }

	[StringLength(100, ErrorMessage = "Email is too long")]
	public string? Email { get; set; }
	public string? LogoUrl { get; set; }

	[StringLength(50, ErrorMessage = "The occupation field is too long")]
	public string? Occupation { get; set; }
}

