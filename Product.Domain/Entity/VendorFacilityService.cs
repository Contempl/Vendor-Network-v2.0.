namespace Product.Domain.Entity;

public class VendorFacilityService : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int VendorFacilityId { get; set; }
    public VendorFacility VendorFacility { get; set; }
}