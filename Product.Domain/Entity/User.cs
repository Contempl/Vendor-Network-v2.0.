namespace Product.Domain.Entity;

public abstract class User : IEntity
{
	public int Id { get; set; }
	public string? UserName { get; set; }
	public byte[]? PasswordHash { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string Email { get; set; }
	public List<Invite> Invites { get; set; } = new();
}
