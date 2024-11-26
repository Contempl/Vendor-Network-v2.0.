using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Product.Application.Dto;
using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;

namespace Product.Infrastructure.Implementations.Account;

public class JwtTokenService : IJwtTokenService  
{
    private readonly JwtOptions _options;
    private readonly IUserRepository _userRepository;
    private readonly IHttpContextAccessor _context;

    public JwtTokenService( IOptions<JwtOptions> options, IUserRepository userRepository, IHttpContextAccessor context)
    {
        _userRepository = userRepository;
        _context = context;
        _options = options.Value;
    }

    public string GenerateToken(User user) 
    {
        var key = Encoding.UTF8.GetBytes(_options.Secret);
        var claims = new List<Claim>
        {
            new Claim("userId", user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email!),
        };
         switch (user) 
        {
            case VendorUser vendorUser:
                claims.Add(new Claim("userType", nameof(VendorUser)));
                claims.Add(new Claim("businessId", vendorUser.VendorId.ToString()));
                break;
            case OperatorUser operatorUser:
                claims.Add(new Claim("userType", nameof(OperatorUser)));
                claims.Add(new Claim("businessId", operatorUser.OperatorId.ToString()));
                break;
            case Administrator administrator:
                claims.Add(new Claim("userType", nameof(Administrator)));
                break;
        };

        var credentials = new SigningCredentials(new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature);

        var jwtSecurityToken = new JwtSecurityToken(
            _options.Issuer,
			_options.Audience,
            claims,
            expires: DateTime.UtcNow.AddHours(_options.ExpireHours),
            signingCredentials: credentials);
        
        var token =  new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        return token;
    }

    private async Task<User> GetUser(int userId)
    {
        var existingUser = await _userRepository.GetByIdAsync(userId);
        return existingUser;
    }
}
