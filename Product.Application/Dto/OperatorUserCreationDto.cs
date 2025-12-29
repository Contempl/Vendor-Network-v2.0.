using Product.Domain.Enum;

namespace Product.Application.Dto;

public record OperatorUserCreationDto
{
    public string Email { get; set; }

    public int OperatorId { get; set; }

    public UserType UserType { get; set; }

    public int CreatedBy { get; set; }
}