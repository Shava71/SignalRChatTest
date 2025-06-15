using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace SignalRChatTest.Service;

public class JwtService
{
    private readonly IConfiguration _configuration;
    private readonly SymmetricSecurityKey _key;
    private readonly string? _issuer;
    private readonly string? _audience;
    
    public JwtService(IConfiguration config, SymmetricSecurityKey securityKey)
    {
        _configuration = config;
        _key = securityKey;
        _issuer = _configuration["Jwt:Issuer"];
        _audience = _configuration["Jwt:Audience"];
    }
    
    public string GenerateToken(Guid userId, string UserName, IList<string> Roles)
    {
        var tokenLifetimeMins = _configuration.GetValue<int>("Jwt:ExpiresInMinutes");

        Claim[] claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, UserName),
        };
        claims = claims.Concat(Roles.Select(role => new Claim(ClaimTypes.Role, role))).ToArray();

        var signingCredentials = new SigningCredentials(
            _key, SecurityAlgorithms.HmacSha256);

        var JwtToken = new JwtSecurityToken
        (
            // Subject = new ClaimsIdentity(new[]
            // {
            //     new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            // }),
            // NotBefore = now,
            // IssuedAt = now,
            // Expires = expires,
            // Issuer = issuer,
            // Audience = audience,
            // signingCredentials = new SigningCredentials(
            //     new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
            //     SecurityAlgorithms.HmacSha256Signature)
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(tokenLifetimeMins),
            signingCredentials: signingCredentials
        );

        // tokenDescriptor.Subject.AddClaims(Roles.Select(r => new Claim(ClaimTypes.Role, r)));

        return new JwtSecurityTokenHandler().WriteToken(JwtToken);
    }
}