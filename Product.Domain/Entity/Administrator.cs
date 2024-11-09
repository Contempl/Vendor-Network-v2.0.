namespace Product.Domain.Entity;

public class Administrator : User, IEntity
{
	public List<Invite> Invites { get; set; } = new();
}
