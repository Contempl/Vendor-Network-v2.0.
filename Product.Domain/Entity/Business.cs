using Product.Domain.Common;

namespace Product.Domain.Entity;

public abstract class Business : IEntityId<int>, IAuditable
{
	public int Id { get; set; }
	public string BusinessName { get; set; }
	public string Address { get; set; }
	public string Email { get; set; }
	
	public List<User> Users { get; set; }
	
	public DateTime CreatedAt { get; set; }
	
	public int CreatedBy { get; set; }
	
	public DateTime? UpdatedAt { get; set; }
	
	public int? UpdatedBy { get; set; }
}
