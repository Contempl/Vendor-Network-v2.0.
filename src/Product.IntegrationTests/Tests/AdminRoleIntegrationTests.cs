using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Enum;

namespace Product.IntegrationTests.Tests;

public class AdminRoleIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AdminRoleIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SuperAdminCreatesVendorAndInvitesItsLocalAdmin()
    {
        var superAdminId = 0;
        await _factory.SeedAsync(async context =>
        {
            var superAdmin = new Administrator { Email = "platform-owner@example.com", UserType = UserType.SuperAdmin };
            context.Administrators.Add(superAdmin);
            await context.SaveChangesAsync();
            superAdminId = superAdmin.Id;
        });

        using var createBusiness = new HttpRequestMessage(HttpMethod.Post, "/Admin/inviteBusiness")
        {
            Content = JsonContent.Create(new
            {
                businessIsVendor = true, businessName = "Example Vendor", businessAddress = "Main Street",
                businessEmail = "vendor@example.com", firstName = "Local", lastName = "Admin",
                userEmail = "local-admin@example.com"
            })
        };
        createBusiness.Headers.Add("X-Test-Role", "SuperAdmin");
        createBusiness.Headers.Add("X-Test-UserId", superAdminId.ToString());
        using var createResponse = await _client.SendAsync(createBusiness);
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        var invitedAdmin = await createResponse.Content.ReadFromJsonAsync<UserDtoToFrontEnd>();
        Assert.True(invitedAdmin?.InviteId > 0);

        var businessId = 0;
        await _factory.SeedAsync(async context =>
        {
            var admin = await context.VendorUsers.SingleAsync(user => user.Id == invitedAdmin!.Id);
            Assert.Equal(UserType.Admin, admin.UserType);
            businessId = admin.VendorId!.Value;
        });

        using var registerResponse = await _client.PostAsJsonAsync($"/Account/Register/User/{invitedAdmin!.InviteId}",
            new { userName = "local-admin", firstName = "Local", lastName = "Admin", password = "test-password" });
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        using var loginRequest = new HttpRequestMessage(HttpMethod.Post, "/Account/Login")
        {
            Content = JsonContent.Create(new { email = "local-admin@example.com", password = "test-password" })
        };
        loginRequest.Headers.Add("X-Test-Anonymous", "true");
        using var loginResponse = await _client.SendAsync(loginRequest);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var token = await loginResponse.Content.ReadFromJsonAsync<TokenDto>();
        var claims = new JwtSecurityTokenHandler().ReadJwtToken(token!.AccessToken).Claims;
        Assert.Equal("Admin", claims.Single(claim => claim.Type == ClaimTypes.Role).Value);
        Assert.Equal("Vendor", claims.Single(claim => claim.Type == "businessType").Value);
        Assert.Equal(businessId.ToString(), claims.Single(claim => claim.Type == "businessId").Value);

        using var updateBusiness = new HttpRequestMessage(HttpMethod.Put, "/Vendor")
        {
            Content = JsonContent.Create(new { businessName = "Updated Vendor", address = "Main Street" })
        };
        SetBusinessAdminHeaders(updateBusiness, invitedAdmin.Id, businessId);
        using var updateResponse = await _client.SendAsync(updateBusiness);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        using var platformRequest = new HttpRequestMessage(HttpMethod.Post, "/Admin/inviteBusiness")
        {
            Content = JsonContent.Create(new { })
        };
        SetBusinessAdminHeaders(platformRequest, invitedAdmin.Id, businessId);
        using var platformResponse = await _client.SendAsync(platformRequest);
        Assert.Equal(HttpStatusCode.Forbidden, platformResponse.StatusCode);

        using var operatorRequest = new HttpRequestMessage(HttpMethod.Get, "/Operator");
        SetBusinessAdminHeaders(operatorRequest, invitedAdmin.Id, businessId);
        using var operatorResponse = await _client.SendAsync(operatorRequest);
        Assert.Equal(HttpStatusCode.Forbidden, operatorResponse.StatusCode);
    }

    [Fact]
    public async Task BusinessMemberCannotEditBusinessOrInviteUsers()
    {
        using var edit = new HttpRequestMessage(HttpMethod.Put, "/Vendor")
        {
            Content = JsonContent.Create(new { businessName = "Member Change", address = "Main Street" })
        };
        edit.Headers.Add("X-Test-Role", "VendorUser");
        using var editResponse = await _client.SendAsync(edit);
        Assert.Equal(HttpStatusCode.Forbidden, editResponse.StatusCode);

        using var invite = new HttpRequestMessage(HttpMethod.Post, "/Vendor/invite")
        {
            Content = JsonContent.Create(new { email = "member@example.com" })
        };
        invite.Headers.Add("X-Test-Role", "VendorUser");
        using var inviteResponse = await _client.SendAsync(invite);
        Assert.Equal(HttpStatusCode.Forbidden, inviteResponse.StatusCode);
    }

    [Fact]
    public async Task LocalVendorAdminCannotDeleteAnotherBusinessesFacility()
    {
        var currentBusinessId = 0;
        var otherBusinessId = 0;
        var facilityId = 0;
        await _factory.SeedAsync(async context =>
        {
            var currentBusiness = new Vendor { BusinessName = "Current Vendor", Address = "First", Email = "current@example.com" };
            var otherBusiness = new Vendor { BusinessName = "Other Vendor", Address = "Second", Email = "other@example.com" };
            context.Vendors.AddRange(currentBusiness, otherBusiness);
            await context.SaveChangesAsync();
            currentBusinessId = currentBusiness.Id;
            otherBusinessId = otherBusiness.Id;

            var facility = new VendorFacility
            {
                Name = "Other facility", Location = "Other location", VendorId = otherBusinessId,
                Vendor = otherBusiness, Services = new List<VendorFacilityService>()
            };
            context.VendorFacilities.Add(facility);
            await context.SaveChangesAsync();
            facilityId = facility.Id;
        });

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete,
            $"/Vendor/{otherBusinessId}/facilities/{facilityId}");
        SetBusinessAdminHeaders(deleteRequest, userId: 42, businessId: currentBusinessId);
        using var response = await _client.SendAsync(deleteRequest);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await _factory.SeedAsync(async context =>
            Assert.NotNull(await context.VendorFacilities.FindAsync(facilityId)));
    }

    [Fact]
    public async Task SuperAdminCreatesOperatorWithLocalAdmin()
    {
        var superAdminId = 0;
        await _factory.SeedAsync(async context =>
        {
            var superAdmin = new Administrator { Email = "operator-platform-owner@example.com", UserType = UserType.SuperAdmin };
            context.Administrators.Add(superAdmin);
            await context.SaveChangesAsync();
            superAdminId = superAdmin.Id;
        });

        using var createBusiness = new HttpRequestMessage(HttpMethod.Post, "/Admin/inviteBusiness")
        {
            Content = JsonContent.Create(new
            {
                businessIsVendor = false, businessName = "Example Operator", businessAddress = "Second Street",
                businessEmail = "operator-business@example.com", firstName = "Operator", lastName = "Admin",
                userEmail = "local-operator-admin@example.com"
            })
        };
        createBusiness.Headers.Add("X-Test-Role", "SuperAdmin");
        createBusiness.Headers.Add("X-Test-UserId", superAdminId.ToString());
        using var createResponse = await _client.SendAsync(createBusiness);
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        var invitedAdmin = await createResponse.Content.ReadFromJsonAsync<UserDtoToFrontEnd>();
        Assert.True(invitedAdmin?.InviteId > 0);
        await _factory.SeedAsync(async context =>
        {
            var admin = await context.OperatorUsers.SingleAsync(user => user.Id == invitedAdmin!.Id);
            Assert.Equal(UserType.Admin, admin.UserType);
            Assert.True(admin.OperatorId > 0);
        });
    }

    private static void SetBusinessAdminHeaders(HttpRequestMessage request, int userId, int businessId)
    {
        request.Headers.Add("X-Test-Role", "Admin");
        request.Headers.Add("X-Test-UserId", userId.ToString());
        request.Headers.Add("X-Test-BusinessId", businessId.ToString());
        request.Headers.Add("X-Test-BusinessType", "Vendor");
    }
}
