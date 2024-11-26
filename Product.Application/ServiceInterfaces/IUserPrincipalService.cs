namespace Product.Application.ServiceInterfaces;

public interface IUserPrincipalService
{
    public int? UserId { get; set; }
    public string? UserType { get; set; }
    public int? BusinessId { get; set; }
}
