using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Moq;
using Product.Domain.Entity;
using Product.Infrastructure.Implementations;
using Xunit;

namespace Product.Tests;

public class EmailServiceTests
{
    [Fact]
    public void GenerateEmailTemplate_ForVendorUser_ReturnsCorrectTemplate()
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();
        var urlHelperMock = new Mock<IUrlHelper>();
        var emailService = new EmailService(configMock.Object, urlHelperMock.Object);

        var user = new VendorUser { Email = "vendor@test.com" };
        var inviteUrl = "https://test.com/invite/123";

        // Act
        var result = emailService.GenerateEmailTemplate(user.Email, user, inviteUrl);

        // Assert
        Assert.Contains("Invitation to Register as a Vendor", result);
        Assert.Contains(inviteUrl, result);
        Assert.Contains(user.Email, result);
    }

    [Fact]
    public void GenerateEmailTemplate_ForOperatorUser_ReturnsCorrectTemplate()
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();
        var urlHelperMock = new Mock<IUrlHelper>();
        var emailService = new EmailService(configMock.Object, urlHelperMock.Object);

        var user = new OperatorUser { Email = "operator@test.com" };
        var inviteUrl = "https://test.com/invite/123";

        // Act
        var result = emailService.GenerateEmailTemplate(user.Email, user, inviteUrl);

        // Assert
        Assert.Contains("Invitation to Register as an Operator business", result);
        Assert.Contains(inviteUrl, result);
        Assert.Contains(user.Email, result);
    }

    [Fact]
    public void GenerateEmailTemplate_ForInvalidUser_ThrowsArgumentException()
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();
        var urlHelperMock = new Mock<IUrlHelper>();
        var emailService = new EmailService(configMock.Object, urlHelperMock.Object);

        var user = new Administrator { Email = "admin@test.com" };
        var inviteUrl = "https://test.com/invite/123";

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            emailService.GenerateEmailTemplate(user.Email, user, inviteUrl));
    }

    [Fact]
    public void CreateInviteUrl_ReturnsCorrectUrl()
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Host"]).Returns("https://test.com");

        var urlHelperMock = new Mock<IUrlHelper>();
        urlHelperMock
            .Setup(u => u.Action(It.IsAny<UrlActionContext>()))
            .Returns("/Account/RegisterUser?inviteId=123");

        var emailService = new EmailService(configMock.Object, urlHelperMock.Object);

        // Act
        var result = emailService.CreateInviteUrl(123);

        // Assert
        Assert.Equal("https://test.com/Account/RegisterUser?inviteId=123", result);
    }

}