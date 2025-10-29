namespace Product.Domain.Common;


public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    
    DateTime? UpdatedAt { get; set; }
    
    int CreatedBy { get; set; }
    
    int? UpdatedBy { get; set; }
}