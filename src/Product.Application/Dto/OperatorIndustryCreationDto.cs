using System.ComponentModel.DataAnnotations;

namespace Product.Application.Dto;

public class OperatorIndustryCreationDto
{
	[Required]
	[StringLength(35, ErrorMessage = "The name of the industry is too long")]
	public string Name { get; set; }

	[Required]
	public string Address { get; set; }

	[Required]
	public double Latitude { get; set; }

	[Required]
	public double Longitude { get; set; }
}