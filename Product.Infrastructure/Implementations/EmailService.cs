using Product.Application.Dto;
using Product.Application.ServiceInterfaces;

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