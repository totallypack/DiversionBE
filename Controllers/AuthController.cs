using Diversion.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Diversion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager) : ControllerBase
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

            var existingUserByEmail = await userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Message = "Registration failed",
                    Errors = ["Email is already taken"]
                });
            }

            var existingUserByUsername = await userManager.FindByNameAsync(model.Username);
            if (existingUserByUsername != null)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Message = "Registration failed",
                    Errors = ["Username is already taken"]
                });
            }

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

            await signInManager.SignInAsync(user, isPersistent: true);

            return Ok(new AuthResponseDto
            {
                Username = user.UserName,
                Email = user.Email
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
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

            var user = await userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                return Unauthorized(new ErrorResponseDto
                {
                    Message = "Invalid username or password",
                    Errors = []
                });
            }

            var result = await signInManager.PasswordSignInAsync(user, model.Password, isPersistent: true, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                return Unauthorized(new ErrorResponseDto
                {
                    Message = "Invalid username or password",
                    Errors = []
                });
            }

            return Ok(new AuthResponseDto
            {
                Username = user.UserName,
                Email = user.Email
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return Ok(new { message = "Logged out successfully" });
        }
    }
}