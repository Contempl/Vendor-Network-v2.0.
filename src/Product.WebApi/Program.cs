using System.Text.Json.Serialization;
using Product.Domain.Settings;
using Product.Infrastructure.Dependency_Injection;
using Product.Infrastructure.Extensions;
using Product.WebApi.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddApiAuthentication(builder.Configuration);
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.ConfigureAuthorization();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(nameof(JwtOptions)));
builder.Services.ConfigureSwagger();

var app = builder.Build();

app.ConfigureApp();

app.Run();


namespace Product.WebApi
{
    public partial class Program;
}