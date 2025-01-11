namespace Product.Domain.Entity;

public class Invite : IEntity
{
    public int Id { get; set; }
    public InvitationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int UserId { get; set; }
    public required User User { get; set; }
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