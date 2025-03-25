using System.Text.Json.Serialization;
using Product.Domain.Settings;
using Product.Infrastructure.Dependency_Injection;
using Product.Infrastructure.Extensions;
using Product.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddApiAuthentication(builder.Configuration);
builder.Services.AddControllers().AddJsonOptions(options => 
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireClaim("userType", "Administrator");
    });
    
    options.AddPolicy("VendorUser", policy =>
    {
        policy.RequireClaim("userType", "VendorUser", "Administrator");
    });
    
    options.AddPolicy("OperatorUser", policy =>
    {
        policy.RequireClaim("userType",  "OperatorUser", "Administrator");
    });
    
    options.AddPolicy("All", policy =>
    {
        policy.RequireClaim("userType",  "VendorUser", "OperatorUser", "Administrator");
    });
});
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(nameof(JwtOptions)));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// builder.Services.AddScoped<IUrlHelper, UrlHelper>();

var app = builder.Build();

app.UseMiddleware<MyExceptionHandlingMiddleware>(); 

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();