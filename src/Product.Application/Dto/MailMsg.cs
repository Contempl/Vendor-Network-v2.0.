namespace Product.Application.Dto;

public record MailMsg(string Body, string Sender)
{
    public int? InviteId { get; init; }
}
