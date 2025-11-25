using Diversion.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Diversion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(UserManager<IdentityUser> userManager, IConfiguration configuration) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Message = "Validation failed",
                    Errors = [.. ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)]
                });
            }

            // Check if email already exists
            var existingUserByEmail = await userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Message = "Registration failed",
                    Errors = ["Email is already taken"]
                });
            }

            // Check if username already exists
            var existingUserByUsername = await userManager.FindByNameAsync(model.Username);
            if (existingUserByUsername != null)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Message = "Registration failed",
                    Errors = ["Username is already taken"]
                });
            }

            // Create new user
            var user = new IdentityUser
            {
                UserName = model.Username,
                Email = model.Email
            };

            var result = await userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Message = "Registration failed",
                    Errors = [.. result.Errors.Select(e => e.Description)]
                });
            }

            // Generate JWT token
            var token = GenerateJwtToken(user);
            var jwtSettings = configuration.GetSection("JwtSettings");
            var expiryMinutes = int.Parse(jwtSettings["ExpiryInMinutes"]);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Username = user.UserName,
                Email = user.Email,
                Expiration = DateTime.UtcNow.AddMinutes(expiryMinutes)
            });
        }

        private string GenerateJwtToken(IdentityUser user)
        {
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expiryMinutes = int.Parse(jwtSettings["ExpiryInMinutes"]);

            var claims = new[]
            {
                  new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                  new Claim(JwtRegisteredClaimNames.Email, user.Email),
                  new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                  new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
              };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}