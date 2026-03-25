namespace Product.Application.Dto;

public class VendorFacilityDto
{
	public string Name { get; init; } = string.Empty;
	public string Location { get; init; } = string .Empty;
	public double Longitude { get; init; }
	public double Latitude { get; init; }
	public double RadiusOfWork { get; init; }
	public List<string>? Services { get; init; } = new();
}
