using System;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interface;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService : ITokenService
{
    public string CreateToken(AppUser user)
    {
       var tokenkey = Config["TokenKey"]?? throw new Exception("Can not access token from appsetting");
       if(tokenkey.Length < 64) throw new Exception("Your tokenkey needs to be longer");
       var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenkey));

       var claims = new List<Claim>
       {
        new(ClaimTypes.NameIdentifier, user.UserName)
       };

       var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

       var tokenDescriptor = new SecurityTokenDescriptor
       {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = creds
       };
    }
}
