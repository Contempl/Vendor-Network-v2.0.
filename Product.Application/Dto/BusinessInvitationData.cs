using System.ComponentModel.DataAnnotations;

namespace Product.Application.Dto;

public class BusinessInvitationData
{
    public bool BusinessIsVendor { get; set; }
    
    [MaxLength(50)]
    public string BusinessName { get; set; }
    
    [MaxLength(50)]
    public string BusinessAddress { get; set; }
    
    [MaxLength(50)]
    public string BusinessEmail { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string UserEmail { get; set; }
}