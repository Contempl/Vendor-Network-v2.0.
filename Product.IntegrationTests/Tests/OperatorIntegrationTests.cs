using System.Net;
using System.Net.Http.Json;
using Product.Application.Dto;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Enum;
using Product.Domain.Pagination;

namespace Product.IntegrationTests.Tests;

public class OperatorIntegrationTests : IntegrationTestBase
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
        var vendorsResponse = await response.Content.ReadFromJsonAsync<BusinessFrontEndDto[]>();
        Assert.True(vendorsResponse!.Length > 0);
    }

    [Fact]
    public async Task SearchVendorsToServeFacilities_ReturnsEmpty_WhenNoVendorsMatchServiceType()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();
        await SeedOperatorWithIndustriesAsync();
        await SeedData();

        var requestDto = new SearchVendorsForIndustriesDto
        {
            IndustriesLocationIds = _seededIndustryIds,
            ServiceType = "plumbing"
        };

        // Act
        var request = await _client.PostAsJsonAsync("/Operator/search/vendors", requestDto);
        var vendors = await request.Content.ReadFromJsonAsync<BusinessFrontEndDto[]>();

        // Assert
        Assert.True(request.IsSuccessStatusCode);
        Assert.Empty(vendors!);
    }

    [Fact]
    public async Task SearchVendorsToServeFacilities_ReturnsEmptyCollection_WhenIndustryIdsDoNotExist()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        var requestDto = new SearchVendorsForIndustriesDto
        {
            IndustriesLocationIds = [999, 1000, 1001],
            ServiceType = "clean"
        };

        // Act
        var request = await _client.PostAsJsonAsync("/Operator/search/vendors", requestDto);
        var vendors = await request.Content.ReadFromJsonAsync<BusinessFrontEndDto[]>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, request.StatusCode);
        Assert.True(vendors!.Length is 0);
    }

    [Fact]
    public async Task SearchVendorsToServeFacilities_ReturnsBadRequest_WhenServiceNameInputIsInvalid()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        var requestDto = new SearchVendorsForIndustriesDto
        {
            IndustriesLocationIds = [1, 2, 3],
            ServiceType = ""
        };

        // Act
        var request = await _client.PostAsJsonAsync("/Operator/search/vendors", requestDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, request.StatusCode);
    }

    [Fact]
    public async Task SearchVendorsToServeFacilities_ReturnsNotFound_WhenIndustryIdsIsEmpty()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        var requestDto = new SearchVendorsForIndustriesDto
        {
            IndustriesLocationIds = [],
            ServiceType = "clean"
        };

        // Act
        var request = await _client.PostAsJsonAsync("/Operator/search/vendors", requestDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, request.StatusCode);
    }

    [Fact]
    public async Task GetVendors_ReturnsPagedVendors_WhenInputIsValid()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();
        await SeedData();

        var requestDto = new VendorSearchDto
        {
            VendorName = "Test" // case sensitive
        };

        // Act
        var request = await _client.PostAsJsonAsync("/Operator/search/vendor", requestDto);
        var pagedVendors = await request.Content.ReadFromJsonAsync<PagedList<Vendor>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, request.StatusCode);
        Assert.NotEmpty(pagedVendors!.Items);
        Assert.Equal(_vendor.BusinessName, pagedVendors.Items[0].BusinessName);
    }

    [Fact]
    public async Task GetVendors_ReturnsEmptyCollection_WhenNoVendorWithGivenNameExists()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();
        await SeedData();

        var requestDto = new VendorSearchDto
        {
            VendorName = "test" // case sensitive
        };

        // Act
        var request = await _client.PostAsJsonAsync("/Operator/search/vendor", requestDto);
        var pagedVendors = await request.Content.ReadFromJsonAsync<PagedList<Vendor>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, request.StatusCode);
        Assert.Empty(pagedVendors!.Items);
    }

    [Fact]
    public async Task GetVendors_ReturnsBadRequest_WhenServiceNameIsInvalid()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();
        await SeedData();

        var requestDto = new VendorSearchDto
        {
            VendorName = ""
        };

        // Act
        var request = await _client.PostAsJsonAsync("/Operator/search/vendor", requestDto);
        var pagedVendors = await request.Content.ReadFromJsonAsync<PagedList<Vendor>>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, request.StatusCode);
    }

    [Fact]
    public async Task GetOperator_ReturnsOperator_WhenInputIsValid()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();
        await SeedOperator();

        // Act
        var request = await _client.GetAsync($"/Operator");
        var obtainedOperator = await request.Content.ReadFromJsonAsync<BusinessFrontEndDto>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, request.StatusCode);
        Assert.Equal(_operator.BusinessName, obtainedOperator!.BusinessName);
    }

    [Fact]
    public async Task GetOperator_Returns500_WhenOperatorDoesntExist()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        // Act
        var request = await _client.GetAsync($"/Operator");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, request.StatusCode);
    }

    [Fact]
    public async Task UpdateOperator_UpdatesOperatorSuccessfully_WhenInputIsValid()
    {
        // Arrange
        await _factory.ResetDatabaseAsync(); 
        await SeedOperator();

        var requestDto = new UpdateOperatorDto
        {
            Address = "Updated Address",
            BusinessName = "Updated BusinessName",
            Email = "New Email"
        };

        // Act
        var request = await _client.PutAsJsonAsync("/Operator", requestDto);
        
        var getResponse = await _client.GetAsync($"/Operator"); // check in db
        var fetchedOperator = await getResponse.Content.ReadFromJsonAsync<BusinessFrontEndDto>();
        
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, request.StatusCode);
        Assert.Equal(requestDto.BusinessName, fetchedOperator!.BusinessName);
    }
    
    [Fact]
    public async Task UpdateOperator_ReturnsError_WhenOperatorDoesntExist()
    {
        // Arrange
        await _factory.ResetDatabaseAsync(); 

        var requestDto = new UpdateOperatorDto
        {
            Address = "Updated Address",
            BusinessName = "Updated BusinessName",
            Email = "New Email"
        };

        // Act
        var request = await _client.PutAsJsonAsync("/Operator", requestDto);
        
        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, request.StatusCode);
    }

    [Fact]
    public async Task InviteOperatorUser_WorksSuccessfully_WithRightInput()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();
        await SeedOperator();
        await SeedOperatorUser();

        var emailDto = new EmailForInviteDto { Email = "testing@mail" };

        // Act
        var request =  await _client.PostAsJsonAsync("/Operator/invite", emailDto);
        var response = await request.Content.ReadFromJsonAsync<MailMsg>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, request.StatusCode);
        Assert.Contains(emailDto.Email, response!.Body);
    }

    private async Task SeedOperatorUser()
    {
        await _factory.SeedAsync(async context =>
        {
            var operatorUser = new OperatorUser
            {
                FirstName = "Test",
                LastName = "User",
                UserType = UserType.OperatorUser,
                Email = "mock@",
                OperatorId = _operator.Id
            };
            await context.AddAsync(operatorUser);
            await context.SaveChangesAsync();
        });
        
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
            _vendor = vendor;
        });

        var vendorFacilities = new VendorFacility[]
        {
            new VendorFacility
            {
                VendorId = _vendor.Id,
                Latitude = 10,
                Longitude = 11,
                Location = "test",
                RadiusOfWork = 500000000,
                Name = "test",
                Services = _vendorFacilities
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
                new OperatorIndustry
                {
                    Name = "clean", Address = "Test", Longitude = 10, Latitude = 11, OperatorId = op.Id, Operator = op
                },
                new OperatorIndustry
                {
                    Name = "wash", Address = "Test", Longitude = 10, Latitude = 11, OperatorId = op.Id, Operator = op
                },
                new OperatorIndustry
                {
                    Name = "create", Address = "Test", Longitude = 10, Latitude = 11, OperatorId = op.Id, Operator = op
                },
            };

            context.OperatorIndustries.AddRange(industries);
            await context.SaveChangesAsync();

            _seededIndustryIds = industries.Select(i => i.Id).ToList();
        });
    }

    private async Task SeedOperator()
    {
        await _factory.SeedAsync(async context =>
        {
            var @operator = new Operator
            {
                Id = 10, // value for our IUserPrincipalService
                BusinessName = "Mock",
                Address = "Mock",
                Email = "moockmail@"
            };
            await context.AddAsync(@operator);
            await context.SaveChangesAsync();
            _operator = @operator;
        });
    }

    private List<int> _seededIndustryIds = new();

    private Operator _operator;

    private Vendor _vendor;
}