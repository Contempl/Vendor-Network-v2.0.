using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Product.Domain.Enum;

namespace Product.IntegrationTests;

public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly CustomWebApplicationFactory _factory;
    protected HttpClient _client;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    protected HttpRequestMessage CreateRequest(
        HttpMethod method,
        string url,
        UserType role,
        int userId = 1,
        int businessId = 10,
        object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("X-Test-Role", role.ToString());
        request.Headers.Add("X-Test-UserId", userId.ToString());
        request.Headers.Add("X-Test-BusinessId", businessId.ToString());

        if (body != null && method != HttpMethod.Get && method != HttpMethod.Head)
        {
            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );
        }

        return request;
    }

    protected HttpRequestMessage AsVendor(HttpMethod method, string url, object? body = null)
        => CreateRequest(method, url, UserType.VendorUser, body: body);

    protected HttpRequestMessage AsOperator(HttpMethod method, string url, object? body = null)
        => CreateRequest(method, url, UserType.OperatorUser, body: body);
}