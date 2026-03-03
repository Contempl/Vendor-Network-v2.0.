using System.Net;
using System.Net.Http.Json;
using Product.Application.Dto;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Enum;

namespace Product.IntegrationTests.Tests;

public class VendorIntegrationTests : IntegrationTestBase
{
    public VendorIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task SearchOperators_ReturnsEmptyList_WhenOperatorsDoNotExist()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();
        
        var dto = new OperatorSearchDto
        {
            Name = "Clean"
        };

        // Act
        var request = await _client.PostAsJsonAsync("/Vendor/Search/Operators/", dto);
        var response =  await request.Content.ReadFromJsonAsync<List<BusinessFrontEndDto>>();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, request.StatusCode);
        Assert.Empty(response!);
    }
    
    [Fact]
    public async Task SearchOperators_ReturnsOperators_WhenOperatorsExist()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();
        await _factory.SeedAsync(async context =>
        {
            context.Operators.Add(new Operator
            {
                BusinessName = "Clean",
                Address = "test",
                Email = "test"
            });
            await context.SaveChangesAsync();
        });

        var requestDto = new OperatorSearchDto
        {
            Name = "Clean"
        };

        // Act
        var request = await _client.PostAsJsonAsync("/Vendor/Search/Operators/", requestDto);

        // Assert
        request.EnsureSuccessStatusCode();

        var result = await request.Content.ReadFromJsonAsync<List<BusinessFrontEndDto>>();

        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task GetVendor_ReturnsVendor_WhenVendorExists()
    {
        // Arrange
        var testVendorId = 10; 

        await _factory.ResetDatabaseAsync();
        await SeedVendorAsync();

        // Act
        var request = await _client.GetAsync($"/Vendor/{testVendorId}"); 
        var obtainedVendor = await request.Content.ReadFromJsonAsync<BusinessFrontEndDto>();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, request.StatusCode);
        Assert.Equal(_vendor.BusinessName, obtainedVendor!.BusinessName);
    }
    
    [Fact]
    public async Task UpdateVendor_UpdatesVendor_WhenVendorExists()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();
        await SeedVendorAsync();

        var updatedVendor = new UpdateVendorDto
        {
            BusinessName = "Updated Vendor",
            Address = "updated",
            Email = "updated",
        };

        // Act
        var request = await _client.PutAsJsonAsync($"/Vendor", updatedVendor); 

        // Assert
        Assert.Equal(HttpStatusCode.OK, request.StatusCode);
        var obtainedVendor = await request.Content.ReadFromJsonAsync<UpdateVendorDto>();
        Assert.Equal(updatedVendor.BusinessName, obtainedVendor!.BusinessName);
    }

    [Fact]
    public async Task UpdateVendor_ReturnsError_WhenVendorDoesntExist()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();
        var updatedVendor = new UpdateVendorDto
        {
            BusinessName = "Updated Vendor",
            Address = "updated",
            Email = "updated",
        };

        // Act
        var request = await _client.PutAsJsonAsync($"/Vendor", updatedVendor); 

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, request.StatusCode);
    }

    [Fact]
    public async Task InviteVendor_ReturnsMailMessage_WhenInviteIsSentSuccessfully()
    {
        // Arrange
        var existingVendorUserMail = "mock@";
        
        await _factory.ResetDatabaseAsync();
        await SeedVendorAsync();
        await SeedUserAsync();

        var emailDto = new EmailForInviteDto { Email = "mock@mail.ru" };

        // Act
        var request = await _client.PostAsJsonAsync("/Vendor/invite", emailDto);
        var obtainedEmailMessage = await request.Content.ReadFromJsonAsync<MailMsg>();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, request.StatusCode);
        Assert.Equal(existingVendorUserMail, obtainedEmailMessage!.Sender);
        Assert.Contains(emailDto.Email, obtainedEmailMessage!.Body);
    }

    [Fact]
    public async Task InviteVendor_ReturnsError_WhenEmailCreationFails()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();
        
        var emailDto = new EmailForInviteDto { Email = "mock@mail.ru" };

        // Act
        var request = await _client.PostAsJsonAsync("/Vendor/invite", emailDto);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, request.StatusCode);
    }
    

    private async Task SeedVendorAsync()
    {
        var vendor = new Vendor
        {
            Id = 10,
            BusinessName = "Test Vendor",
            Address = "popa",
            Email = "mock",
        };
        
        await _factory.SeedAsync(async context =>
        {
            context.Add(vendor);
            await context.SaveChangesAsync();
            _vendor = vendor;
        });
    }

    private Vendor _vendor;
    
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