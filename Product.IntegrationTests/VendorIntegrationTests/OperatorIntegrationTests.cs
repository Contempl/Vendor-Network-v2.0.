using System.Net.Http.Json;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Enum;

namespace Product.IntegrationTests.VendorIntegrationTests;

public class OperatorIntegrationTests :  IntegrationTestBase
{
    public OperatorIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
    {
        _client = factory.CreateClient(); 
        _client.DefaultRequestHeaders.Add("X-Test-Role", UserType.OperatorUser.ToString());
        _client.DefaultRequestHeaders.Add("X-Test-UserId", "1");
        _client.DefaultRequestHeaders.Add("X-Test-BusinessId", "10");
    }

    [Fact]
    public async Task SearchVendorsToServeFacilities_ReturnsVendors_WhenVendorsFacilitiesReach()
    {
        // Arrange
        await SeedOperatorWithIndustriesAsync();
        await SeedData();
        var requestDto = new SearchVendorsForIndustriesDto
        {
            IndustriesLocationIds = _seededIndustryIds,
            ServiceType = "clean"
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/Operator/search/vendors", requestDto);


        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var vendors = await response.Content.ReadFromJsonAsync<BusinessFrontEndDto[]>();
        Assert.True(vendors!.Length > 0);
    }

    private async Task SeedData()
    {
        var vendor = new Vendor
        {
            BusinessName = "Test",
            Address = "Test",
            Email = "email"
        };

        await _factory.SeedAsync(async context =>
        {
            context.Vendors.Add(vendor);
            await context.SaveChangesAsync();
        });

        var vendorFacilities = new VendorFacility[]
        {
            new VendorFacility
            {
                VendorId = 1, Latitude = 10, Longitude = 11, Location = "test", RadiusOfWork = 500000000, Name = "test", Services = _vendorFacilities
            }
        };

        await _factory.SeedAsync(async context =>
        {
            context.VendorFacilities.AddRange(vendorFacilities);
            await context.SaveChangesAsync();
        });
    }

    private readonly List<VendorFacilityService> _vendorFacilities = new()
    {
        new VendorFacilityService { Name = "clean" },
        new VendorFacilityService { Name = "wash" },
        new VendorFacilityService { Name = "create" }
    };
    
    private List<int> _seededIndustryIds = new();
    
    private async Task SeedOperatorWithIndustriesAsync()
    {
        await _factory.SeedAsync(async context =>
        {
            var op = new Operator
            {
                Address = "Test",
                Email = "email",
                BusinessName = "Test",
                Occupation = "Mock"
            };

            context.Operators.Add(op);
            await context.SaveChangesAsync();

            var industries = new List<OperatorIndustry>
            {
                new OperatorIndustry { Name = "clean", Address = "Test", Longitude = 10, Latitude = 12, OperatorId = op.Id, Operator = op},
                new OperatorIndustry { Name = "wash",  Address = "Test", Longitude = 10, Latitude = 12, OperatorId = op.Id, Operator = op},
                new OperatorIndustry { Name = "create", Address = "Test", Longitude = 10, Latitude = 12, OperatorId = op.Id, Operator = op },
            };

            context.OperatorIndustries.AddRange(industries);
            await context.SaveChangesAsync();
            
            _seededIndustryIds = industries.Select(i => i.Id).ToList();
        });
    }
}