using Product.Domain.Common;

namespace Product.Domain.Entity;

public class VendorFacilityService : IEntityId<int>, IAuditable
{
    public int Id { get; set; }
    public string Name { get; set; } 
    public int VendorFacilityId { get; set; }
    public VendorFacility VendorFacility { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public int CreatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public int? UpdatedBy { get; set; }
}