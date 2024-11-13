namespace Product.Infrastructure.Dto;

public class UserDtoToFrontEnd
{
	public int Id { get; set; }
	required public string UserName { get; set; }
	required public string FirstName { get; set; }
	required public string LastName { get; set; }
	required public string Email { get; set; }
}
