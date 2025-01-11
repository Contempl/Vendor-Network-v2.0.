using Product.Domain.Entity;

namespace Product.Application.Dto;

public class BusinessInvitationData
{
    public Business Business { get; set; }
    public string BusinessName { get; set; }
    public string BusinessAddress { get; set; }
    public string BusinessEmail { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string UserEmail { get; set; }
}