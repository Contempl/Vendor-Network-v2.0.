using System.ComponentModel.DataAnnotations;

namespace Product.Application.Dto;

public class UserLoginDto
{
    [Required(AllowEmptyStrings = false)]
    public string Email { get; set; } = string.Empty;

	[Required(AllowEmptyStrings = false)]
	public string Password { get; set; } = string.Empty;
}
