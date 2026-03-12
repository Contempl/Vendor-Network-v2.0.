using Product.Domain.Common;

namespace Product.Domain.Entity;

public class OperatorIndustry : IEntityId<int>, IAuditable
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public required Operator Operator { get; set; }
    public int OperatorId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public int CreatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public int? UpdatedBy { get; set; }
}