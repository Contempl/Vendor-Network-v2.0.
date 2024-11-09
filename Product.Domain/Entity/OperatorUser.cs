namespace Product.Domain.Entity;

public class OperatorUser : User, IEntity
{
    public Operator? Operator { get; set; }
    public int? OperatorId { get; set; }
}
