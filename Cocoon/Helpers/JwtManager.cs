using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Configuration;

public class JwtManager
{

    public static string GenerateToken(string Id, string Email, string role = "User")
    {
        // Get the key from configuration (ensure it's at least 256 bits long)
        var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Convert.ToString(ConfigurationManager.AppSettings["config:JwtKey"])));

        var tokenHandler = new JwtSecurityTokenHandler();

        // Define the claims that will be included in the JWT token
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.NameIdentifier, Id),
            new Claim(ClaimTypes.Name, Email),
            new Claim(ClaimTypes.Role, role),
        }),
            Expires = DateTime.UtcNow.AddDays(Convert.ToInt32(Convert.ToString(ConfigurationManager.AppSettings["config:JwtDurationInDays"]))),

            // Add Issuer and Audience for additional security and validation
            Issuer = Convert.ToString(ConfigurationManager.AppSettings["config:JwtValidIssuer"]),
            Audience = Convert.ToString(ConfigurationManager.AppSettings["config:JwtValidAudiance"]),

            // Use SigningCredentials to specify the symmetric key and the HMAC SHA256 algorithm
            SigningCredentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256Signature)
        };

        // Create the JWT token
        var stoken = tokenHandler.CreateToken(tokenDescriptor);

        // Return the generated token
        var token = tokenHandler.WriteToken(stoken);

        return token;
    }
    // Method to validate a refresh token and extract claims
    public static ClaimsPrincipal ValidateRefreshToken(string refreshToken)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(Convert.ToString(ConfigurationManager.AppSettings["config:JwtKey"]));

            // Token validation parameters
            var validationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidIssuer = Convert.ToString(ConfigurationManager.AppSettings["config:JwtValidIssuer"]),
                ValidAudience = Convert.ToString(ConfigurationManager.AppSettings["config:JwtValidAudiance"]),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false, // You can set this to true if you want to validate expiration as well
            };

            // Validate the token and extract the claims
            var principal = tokenHandler.ValidateToken(refreshToken, validationParameters, out SecurityToken validatedToken);

            return principal; // Returns ClaimsPrincipal containing user claims (Id, Email, etc.)
        }
        catch (Exception)
        {
            return null; // Invalid token or error in validation
        }
    }
}
