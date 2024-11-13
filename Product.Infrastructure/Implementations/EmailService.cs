using Product.Application.ServiceInterfaces;
using Product.Infrastructure.Dto;

namespace Product.Infrastructure.Implementations;

public class EmailService : IEmailService
{
    public Task SendInvitationEmailAsync(MailMsg mailMessage)
    {
        return Task.CompletedTask;
    }

    public MailMsg CreateMessage(string emailBody, string senderEmail)
    {
        return new MailMsg(emailBody, senderEmail);
    }
}