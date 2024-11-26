using Product.Application.Dto;

namespace Product.Application.ServiceInterfaces;

public interface IEmailService
{
	Task SendInvitationEmailAsync(MailMsg mailMessage);
	MailMsg CreateMessage(string emailBody, string senderEmail);
}