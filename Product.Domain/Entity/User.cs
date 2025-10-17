using System.Text.Json.Serialization;
using Product.Domain.Common;
using Product.Domain.Enum;

namespace Product.Domain.Entity;

[JsonDerivedType(typeof(VendorUser), "vendorUser")]
[JsonDerivedType(typeof(OperatorUser), "operatorUser")]
public abstract class User : IEntityId<int>, IAuditable
{
	public int Id { get; set; }
	
	public string? UserName { get; set; }
	
	public byte[]? PasswordHash { get; set; }
	
	public string? FirstName { get; set; }
	
	public string? LastName { get; set; }
	
	public string Email { get; set; }

	public UserType UserType { get; set; }

	public List<RefreshToken> RefreshTokens { get; set; } = new();
	
	public List<Invite> SentInvites { get; set; } = new();
	
	public Invite? ReceivedInvite { get; set; }
	
	public DateTime CreatedAt { get; set; }
	
	public int CreatedBy { get; set; }
	
	public DateTime? UpdatedAt { get; set; }
	
	public int? UpdatedBy { get; set; }
}
