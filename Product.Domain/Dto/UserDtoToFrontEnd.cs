namespace Product.Domain.Dto;

public class UserDtoToFrontEnd
{
	public int Id { get; set; }
	public string FirstName { get; set; }
	public required string Email { get; set; }
}
