using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;
using Product.Infrastructure.Dto;

namespace Product.Infrastructure.Implementations.Account;

public class JwtTokenService : IJwtTokenService  
{
    private readonly JwtOptions _options;
    private readonly IUserRepository _userRepository;

    public JwtTokenService( IOptions<JwtOptions> options, IUserRepository userRepository)
    {
        _userRepository = userRepository;
        _options = options.Value;
    }

    public string GenerateToken(Domain.Entity.User user) 
    {
        var key = Encoding.UTF8.GetBytes(_options.Secret);
        var userType = user switch 
        {
            VendorUser => "VendorUser",
            OperatorUser => "OperatorUser",
            Administrator => "Administrator",
        };
        var claims = new Claim[]
        {
            new Claim("userId", user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim("userType", userType),
        };

        var credentials = new SigningCredentials(new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature);

        var token = new JwtSecurityToken(
            _options.Issuer,
			_options.Audience,
            claims,
            expires: DateTime.UtcNow.AddHours(_options.ExpireHours),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string ParseToken(string jwtToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(jwtToken);
        var usernameClaim = token.Claims.First(c => c.Type == ClaimTypes.Name).Value;
        return usernameClaim;
    }

    private async Task<User> GetUser(int userId)
    {
        var existingUser = await _userRepository.GetByIdAsync(userId);
        return existingUser;
    }
}
