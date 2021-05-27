using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ZoomJWAssistant.Core
{
    class JWTTokenCreator
    {
        public static string CreateToken(string apiKey, string apiSecret)
        {
           // Create Security key  using private key above:
           // not that latest version of JWT using Microsoft namespace instead of System
            var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(apiSecret));

            // Also note that securityKey length should be >256b
            // so you have to make sure that your private key has a proper length
            //
            var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            //  Finally create a Token
            var header = new JwtHeader(credentials);

            var iat = (int) (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            var exp = (int) (DateTime.UtcNow.AddDays(1) - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            var tokenExp = (int) (DateTime.UtcNow.AddHours(1) - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

            //Some PayLoad that contain information about the  customer
            var payload = new JwtPayload
           {
               { "appKey", apiKey},
               { "iat", iat},
               { "exp", exp},
               { "tokenExp", tokenExp},
           };

            //
            var secToken = new JwtSecurityToken(header, payload);
            var handler = new JwtSecurityTokenHandler();

            // Token to String so you can use it in your client
            var token = handler.WriteToken(secToken);
            //Console.WriteLine("TOKEN: " + token);
            return token;
        }

    }
}
