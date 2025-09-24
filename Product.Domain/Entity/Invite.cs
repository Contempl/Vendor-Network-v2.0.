using Product.Domain.Common;

namespace Product.Domain.Entity;

public class Invite : IEntityId<int>, IAuditable
{
    public int Id { get; set; }
    public InvitationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public int CreatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public int? UpdatedBy { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? InvitedUserId { get; set; }
    public User? InvitedUser { get; set; }
    public int SenderId { get; set; }
    public User Sender { get; set; }
}

public enum InvitationStatus
{
    Sent,
    Accepted,
    Declined,
    Expired
}