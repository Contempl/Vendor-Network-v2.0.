namespace Product.Domain.Dto;

public record InviteDtoWithStatus
{
    public int InviteId { get; set; }

    public string Status { get; set; }
}