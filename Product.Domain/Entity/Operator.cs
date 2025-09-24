namespace Product.Domain.Entity;

public class Operator : Business
{
	public string? LogoUrl { get; set; }
	public string Occupation { get; set; }
	public List<OperatorIndustry>? Industries { get; set; }
	public List<OperatorUser> OperatorUsers { get; set; }
}
