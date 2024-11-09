namespace Product.Domain.Entity;

public class OperatorIndustry : IEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public required Operator Operator { get; set; }
    public int OperatorId { get; set; }
}