using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Settings;

namespace Product.Infrastructure.Implementations.Account;

public class JwtTokenService : IJwtTokenService  
{
    private readonly JwtOptions _options;
    public JwtTokenService( IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public TokenDto GenerateToken(UserClaimDto userClaim) 
    {
        var key = Encoding.UTF8.GetBytes(_options.Secret);
        var claims = new List<Claim>
        {
            new Claim("userId", userClaim.Id.ToString()),
            new Claim(ClaimTypes.Email, userClaim.Email!),
            new Claim(ClaimTypes.Role, userClaim.UserType.ToString()),
            new Claim("businessId", userClaim.BusinessId.ToString()),
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
        var refreshToken = GenerateRefreshToken();
        
        return new TokenDto
        {
            AccessToken = token,
            RefreshToken = refreshToken, 
        };
    }

    public string GenerateRefreshToken() 
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    
    public bool Validate(RefreshToken token)
        => token.ExpiresAt > DateTime.UtcNow && !token.Revoked;
}
