using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Google.Apis.Auth;

namespace Futebol.Api.Utils
{
    public static class Security
    {
        public static string HashSenha(string senha)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(senha));
            return Convert.ToBase64String(bytes);
        }

        public static string GenerateJwt(Guid userId, string role, IConfiguration config, TimeSpan? expiration = null)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim("role", role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.Now.AddTicks((expiration ?? TimeSpan.FromDays(1)).Ticks);

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static async Task<GoogleJsonWebSignature.Payload?> VerifyGoogleToken(string token, string clientId)
        {
            try
            {
                GoogleJsonWebSignature.Payload payload;
                if (!string.IsNullOrWhiteSpace(clientId))
                {
                    var settings = new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { clientId }
                    };
                    payload = await GoogleJsonWebSignature.ValidateAsync(token, settings);
                }
                else
                {
                    payload = await GoogleJsonWebSignature.ValidateAsync(token);
                }
                return payload;
            }
            catch
            {
                return null;
            }
        }
    }
}
