using Product.Application.Dto;
using Product.Domain.Entity;

namespace Product.Application.ServiceInterfaces;

public interface IEmailService
{
	Task SendInvitationEmailAsync(MailMsg mailMessage);
	MailMsg CreateMessage(string emailBody, string senderEmail);
	string GenerateEmailTemplate(string email, User user, string inviteUrl);
	string CreateInviteUrl(int inviteId);
}
