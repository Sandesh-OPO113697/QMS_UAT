using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace QMS.Models
{
    public class JWTHelper
    {
        public static SymmetricSecurityKey _signinkey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MbQeThWmZq4t7w!zMbQeThWmZq4t7w!z")); 

        public static string CreateJWTToken(string userid, string role)
        {
            var credentials = new SigningCredentials(_signinkey, SecurityAlgorithms.HmacSha256);
            var issuer = "https://localhost:5023/";
            var audiance = "https://localhost:5023/";
            var claims = new[] {

                new Claim(ClaimTypes.Name,userid),
                new Claim(ClaimTypes.Role,role)

             };
                
            var token = new JwtSecurityToken(
                    issuer,
                    audiance,
                    claims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: credentials
                    );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public static IPrincipal ValidatejwtToken(string token)
        {
            var h = new JwtSecurityTokenHandler();

            h.ValidateToken(token, new TokenValidationParameters()
            {

                ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
                ValidateAudience = true,
                ValidateIssuer = true,    
                ValidAudience = "https://localhost:5023/",
                ValidIssuer = "https://localhost:5023/",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MbQeThWmZq4t7w!zMbQeThWmZq4t7w!z")),
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true
            }, out SecurityToken securityToken);

            JwtSecurityToken jwt = (JwtSecurityToken)securityToken;
            var id = new ClaimsIdentity(jwt.Claims, "jwt", ClaimTypes.Name, ClaimTypes.Role);
            return new ClaimsPrincipal(id);
        }
        public static void AuthenticationRequest(string token , HttpContext context)
        {
            try
            {
                var principal = ValidatejwtToken(token);
                context.User = (ClaimsPrincipal)principal;
                Thread.CurrentPrincipal = principal;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

    }

    
}
