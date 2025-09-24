using Product.Domain.Enum;

namespace Product.Domain.Dto;

public record UserClaimDto
{
    public int Id { get; set; }

    public string Email { get; set; }

    public UserType UserType { get; set; }

    public int? BusinessId { get; set; }
}