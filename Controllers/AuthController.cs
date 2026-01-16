using Diversion.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Diversion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager) : ControllerBase
    {
        /// <summary>
        /// Registers a new user account
        /// </summary>
        /// <param name="model">Registration data including username, email, and password</param>
        /// <returns>Authentication response with user details</returns>
        /// <remarks>
        /// Password requirements: minimum 8 characters, at least one uppercase, one lowercase, and one digit.
        /// Automatically signs in the user upon successful registration.
        /// </remarks>
        /// <response code="200">Registration successful, user is signed in</response>
        /// <response code="400">Validation failed (email/username taken or invalid data)</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
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

        /// <summary>
        /// Authenticates a user and creates a session
        /// </summary>
        /// <param name="model">Login credentials (username and password)</param>
        /// <returns>Authentication response with user details</returns>
        /// <remarks>
        /// Creates a persistent cookie-based session (7-day expiration with sliding window).
        /// </remarks>
        /// <response code="200">Login successful, session cookie set</response>
        /// <response code="400">Validation failed</response>
        /// <response code="401">Invalid credentials</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
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

        /// <summary>
        /// Signs out the current user and invalidates the session
        /// </summary>
        /// <returns>Success message</returns>
        /// <response code="200">Logout successful</response>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return Ok(new { message = "Logged out successfully" });
        }
    }
}