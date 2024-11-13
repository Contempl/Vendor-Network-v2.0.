using System.ComponentModel.DataAnnotations;

namespace Product.Infrastructure.Dto;

public class UpdateOperatorIndustryDto
{

	[StringLength(35, ErrorMessage = "The name of the facility is too long")]
	public string? Name { get; set; }

	public string? Address { get; set; }

	public double? Latitude { get; set; }

	public double? Longitude { get; set; }
}