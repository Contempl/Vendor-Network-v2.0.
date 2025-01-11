namespace Product.Domain.Entity;

public class VendorUser : User
{
    public Vendor? Vendor { get; set; }
    public int? VendorId { get; set; }
}
