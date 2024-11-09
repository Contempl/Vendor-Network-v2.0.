namespace Product.Domain.Entity;

public class VendorUser : User, IEntity
{
    public Vendor? Vendor { get; set; }
    public int? VendorId { get; set; }
}
