using Microsoft.Extensions.Configuration;
using Product.Application.Dto;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;
using Microsoft.AspNetCore.Mvc;

namespace Product.Infrastructure.Implementations;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly IUrlHelper _urlHelper;
    public EmailService(IConfiguration configuration, IUrlHelper urlHelper)
    {
        _configuration = configuration;
        _urlHelper = urlHelper;
    }

    public Task SendInvitationEmailAsync(MailMsg mailMessage)
    {
        return Task.CompletedTask;
    }

    public MailMsg CreateMessage(string emailBody, string senderEmail)
    {
        return new MailMsg(emailBody, senderEmail);
    }

    public string GenerateEmailTemplate(string email, User user, string inviteUrl)
    {
        string emailTemplate = "";

        if (user is VendorUser)
            emailTemplate =
                $"<h1>Invitation to Register as a Vendor</h1>" +
                $"<p>Dear {email},</p>" +
                $"<p>You have been invited to register as a Vendor business. Please click the following link to complete your registration:</p>" +
                $"<a href=\"{inviteUrl}\">Register now</a>" +
                $"<p>This invitation will expire in 30 days.</p>";

        else if (user is OperatorUser)
            emailTemplate =
                $"<h1>Invitation to Register as an Operator business</h1>" +
                $"<p>Dear {email},</p>" +
                $"<p>You have been invited to register your business. Please click the following link to complete your registration:</p>" +
                $"<a href=\"{inviteUrl}\">Register now</a>" +
                $"<p>This invitation will expire in 30 days.</p>";

        else
            throw new ArgumentException($"Invalid User Type");

        return emailTemplate;
    }

    public string CreateInviteUrl(int inviteId)
    {
        var host = _configuration["Host"];

        return string.Concat(host, _urlHelper.Action("RegisterUser", "Account", new { inviteId }));
    }
}