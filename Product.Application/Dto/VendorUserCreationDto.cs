using Product.Domain.Enum;

namespace Product.Application.Dto;

public record VendorUserCreationDto
{
    public string Email { get; set; }

    public int VendorId { get; set; }

    public UserType UserType { get; set; }

    public int CreatedBy { get; set; }
}