using Product.Domain.Enum;

namespace Product.Domain.Entity;

public abstract class User : IEntity
{
	public int Id { get; set; }
	public string? UserName { get; set; }
	public byte[]? PasswordHash { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string Email { get; set; }

	public UserType UserType { get; set; }
	
	public List<Invite> SentInvites { get; set; } = new();
	
	public Invite? ReceivedInvite { get; set; }
}
