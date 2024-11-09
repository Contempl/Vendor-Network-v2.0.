namespace Product.Domain.Entity;

public class Vendor : Business, IEntity
{
    public List<VendorFacility> VendorFacilities { get; set; } = new List<VendorFacility>();
    public List<VendorUser> VendorUsers { get; set; } = new List<VendorUser>();
}
