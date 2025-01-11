namespace Product.Domain.Entity;

public class OperatorUser : User
{
    public Operator? Operator { get; set; }
    public int? OperatorId { get; set; }
}
