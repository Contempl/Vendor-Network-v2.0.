using System.Net;
using System.Net.Http.Json;
using Product.Application.Dto;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Enum;

namespace Product.IntegrationTests.Tests;

public class VendorIntegrationTests : IntegrationTestBase, IAsyncLifetime
{
    public VendorIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
    {
        _client.DefaultRequestHeaders.Add("X-Test-Role", UserType.Admin.ToString());
        _client.DefaultRequestHeaders.Add("X-Test-BusinessType", "Vendor");
    }

    [Fact]
    public async Task SearchOperators_ReturnsEmptyList_WhenOperatorsDoNotExist()
    {
        // Arrange
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
        var vendor = new Vendor
        {
            Id = 10,
            BusinessName = "Test Vendor",
            Address = "popa",
            Email = "mock",
        };

        await SeedVendorAsync(vendor);

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
        var vendor = new Vendor
        {
            Id = 10,
            BusinessName = "Test Vendor",
            Address = "popa",
            Email = "mock",
        };
        
        await SeedVendorAsync(vendor);

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
    public async Task CreateFacility_AddsFacilityWithServicesForCurrentVendor()
    {
        await SeedVendorAsync(new Vendor
        {
            Id = 10, BusinessName = "Test Vendor", Address = "Main Street", Email = "vendor@example.com"
        });

        var response = await _client.PostAsJsonAsync("/facility", new VendorFacilityDto
        {
            Name = "Main facility", Location = "Main Street", RadiusOfWork = 10,
            Services = new List<string> { "Delivery" }
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var facilitiesResponse = await _client.GetAsync("/Vendor/facilities");
        Assert.Equal(HttpStatusCode.OK, facilitiesResponse.StatusCode);
        var facilities = await facilitiesResponse.Content.ReadFromJsonAsync<List<VendorGetFacilitiesDto>>();
        var facility = Assert.Single(facilities!);
        Assert.Equal("Main facility", facility.Name);
        Assert.Equal("Delivery", Assert.Single(facility.Services!));
    }

    [Fact]
    public async Task InviteVendor_ReturnsMailMessage_WhenInviteIsSentSuccessfully()
    {
        // Arrange
        var existingVendorUserMail = "mock@";
        var vendor = new Vendor
        {
            Id = 10,
            BusinessName = "Test Vendor",
            Address = "popa",
            Email = "mock",
        };

        var vendorUser = new VendorUser
        {
            FirstName = "Test User",
            LastName = "Test",
            UserType = UserType.VendorUser,
            Email = "mock@",
        };
        

        await SeedVendorAsync(vendor);
        await SeedUserAsync(vendorUser);

        var emailDto = new EmailForInviteDto { Email = "mock@mail.ru" };

        // Act
        var request = await _client.PostAsJsonAsync("/Vendor/invite", emailDto);
        var obtainedEmailMessage = await request.Content.ReadFromJsonAsync<MailMsg>();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, request.StatusCode);
        Assert.Equal(existingVendorUserMail, obtainedEmailMessage!.Sender);
        Assert.Contains(emailDto.Email, obtainedEmailMessage!.Body);
        Assert.True(obtainedEmailMessage.InviteId > 0);
    }

    [Fact]
    public async Task InviteVendor_ReturnsError_WhenEmailCreationFails()
    {
        // Arrange
        var emailDto = new EmailForInviteDto { Email = "mock@mail.ru" };

        // Act
        var request = await _client.PostAsJsonAsync("/Vendor/invite", emailDto);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, request.StatusCode);
    }
    

    private async Task SeedVendorAsync(Vendor vendor)
    {
        await _factory.SeedAsync(async context =>
        {
            context.Add(vendor);
            await context.SaveChangesAsync();
            _vendor = vendor;
        });
    }

    private Vendor _vendor;
    
    private async Task SeedUserAsync(VendorUser vendorUser)
    {
        await _factory.SeedAsync(async context =>
        {
            context.Add(vendorUser);
            await context.SaveChangesAsync();
        });
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
