using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace App.Services;

public class TokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        string jwtKey =  _config["JWT_KEY"] ?? throw new Exception("JWT_KEY not set in .env.");
        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        DateTime expires = DateTime.UtcNow.AddHours(1);

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: _config["JWT_ISSUER"],
            audience: _config["JWT_AUDIENCE"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}