using Product.Domain.Enum;

namespace Product.Application.ServiceInterfaces;

public interface IUserPrincipalService
{
    public int? UserId { get; set; }
    public UserType? UserType { get; set; }
    public int? BusinessId { get; set; }
}
