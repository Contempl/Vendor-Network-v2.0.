using System.Net;
using System.Net.Http.Json;
using Product.Application.Dto;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Enum;

namespace Product.IntegrationTests.VendorIntegrationTests;

public class VendorIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public VendorIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _factory = factory;
    }

    [Fact]
    public async Task SearchOperators_ReturnsEmptyList_WhenOperatorsDoNotExist()
    {
        // Arrange
        var request = new OperatorSearchDto
        {
            Name = "Clean"
        };

        // Act
        var result = await _client.PostAsJsonAsync("/Vendor/Search/Operators/", request);
        var response =  await result.Content.ReadFromJsonAsync<List<BusinessFrontEndDto>>();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Empty(response!);
    }
    
    [Fact]
    public async Task SearchOperators_ReturnsOperators_WhenOperatorsExist()
    {
        // Arrange
        await SeedVendorAsync();

        var request = new OperatorSearchDto
        {
            Name = "Clean"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/Vendor/Search/Operators/", request);

        // Assert
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<List<BusinessFrontEndDto>>();

        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task GetVendor_ReturnsVendor_WhenVendorExists()
    {
        // Arrange
        var testVendorId = 10; 

        await SeedVendorAsync();

        // Act
        var response = await _client.GetAsync($"/Vendor/{testVendorId}"); 
        var obtainedVendor = await response.Content.ReadFromJsonAsync<BusinessFrontEndDto>();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Test Vendor", obtainedVendor!.BusinessName);
    }
    
    [Fact]
    public async Task UpdateVendor_UpdatesVendor_WhenVendorExists()
    {
        // Arrange
        await SeedVendorAsync();

        var updatedVendor = new UpdateVendorDto
        {
            BusinessName = "Updated Vendor",
            Address = "updated",
            Email = "updated",
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/Vendor", updatedVendor); 

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var obtainedVendor = await response.Content.ReadFromJsonAsync<UpdateVendorDto>();
        Assert.Equal(updatedVendor.BusinessName, obtainedVendor!.BusinessName);
    }

    [Fact]
    public async Task UpdateVendor_ReturnsError_WhenVendorDoesntExist()
    {
        // Arrange
        var updatedVendor = new UpdateVendorDto
        {
            BusinessName = "Updated Vendor",
            Address = "updated",
            Email = "updated",
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/Vendor", updatedVendor); 

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task InviteVendor_ReturnsMailMessage_WhenInviteIsSentSuccessfully()
    {
        // Arrange
        var existingVendorUserMail = "mock@";
        await SeedVendorAsync();
        await SeedUserAsync();

        var emailDto = new EmailForInviteDto { Email = "mock@mail.ru" };

        // Act
        var response = await _client.PostAsJsonAsync("/Vendor/invite", emailDto);
        var obtainedEmailMessage = await response.Content.ReadFromJsonAsync<MailMsg>();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(existingVendorUserMail, obtainedEmailMessage!.Sender);
        Assert.Contains(emailDto.Email, obtainedEmailMessage!.Body);
    }

    [Fact]
    public async Task InviteVendor_ReturnsError_WhenEmailCreationFails()
    {
        // Arrange
        var emailDto = new EmailForInviteDto { Email = "mock@mail.ru" };

        // Act
        var response = await _client.PostAsJsonAsync("/Vendor/invite", emailDto);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
    

    private async Task SeedVendorAsync()
    {
        await _factory.SeedAsync(async context =>
        {
            context.Add(new Vendor
            {
                Id = 10,
                BusinessName = "Test Vendor",
                Address = "popa",
                Email = "mock",
            });
            await context.SaveChangesAsync();
        });
    }
    
    private async Task SeedUserAsync()
    {
        await _factory.SeedAsync(async context =>
        {
            context.Add(new VendorUser
            {
                FirstName = "Test User",
                LastName =  "Test",
                UserType = UserType.VendorUser,
                Email = "mock@",
            });
            await context.SaveChangesAsync();
        });
    }
}