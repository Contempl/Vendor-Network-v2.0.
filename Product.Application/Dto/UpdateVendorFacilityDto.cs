namespace Product.Application.Dto;

public class UpdateVendorFacilityDto
{
	public string? Name { get; set; }
	public string? Location { get; set; }
	public double? Longitude { get; set; }
	public double? Latitude { get; set; }
	public double? RadiusOfWork { get; set; }
	public List<string>? Services { get; set; }
}
