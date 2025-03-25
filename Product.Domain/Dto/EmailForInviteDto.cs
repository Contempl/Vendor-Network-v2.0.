using System.ComponentModel.DataAnnotations;

namespace Product.Domain.Dto;

public class EmailForInviteDto
{
    [Required]
    public string Email { get; set; }
}