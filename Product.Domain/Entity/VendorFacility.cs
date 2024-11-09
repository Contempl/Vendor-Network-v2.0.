namespace Product.Domain.Entity;

public class VendorFacility : IEntity
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string Location { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public int VendorId { get; set; }
    public Vendor Vendor { get; set; }
    public double RadiusOfWork { get; set; }
    public List<VendorFacilityService> Services { get; set; }
}
