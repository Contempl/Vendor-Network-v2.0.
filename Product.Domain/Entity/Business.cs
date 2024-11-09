namespace Product.Domain.Entity;

public abstract class Business : IEntity
{
	public int Id { get; set; }
	public string BusinessName { get; set; }
	public string Adress { get; set; }
	public string Email { get; set; }
}
