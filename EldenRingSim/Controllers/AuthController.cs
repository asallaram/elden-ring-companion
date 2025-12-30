using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using EldenRingSim.Models;
using EldenRingSim.Services;

// Authentication endpoints for user registration, login, and profile management with JWT token generation

namespace EldenRingSim.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtTokenService jwtTokenService,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }

   
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "User with this email already exists" });
                }

                // Create new user
                var user = new ApplicationUser
                {
                    UserName = request.Username,
                    Email = request.Email,
                    PsnId = request.PsnId,
                    Playstyle = request.Playstyle,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    return BadRequest(new
                    {
                        message = "Failed to create user",
                        errors = result.Errors.Select(e => e.Description)
                    });
                }

                _logger.LogInformation("User {Username} registered successfully", user.UserName);

                var token = _jwtTokenService.GenerateToken(user);

                return Ok(new
                {
                    message = "Registration successful",
                    userId = user.Id,
                    username = user.UserName,
                    email = user.Email,
                    token = token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, new { message = "An error occurred during registration" });
            }
        }

      
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.EmailOrUsername)
                    ?? await _userManager.FindByNameAsync(request.EmailOrUsername);

                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

                if (!result.Succeeded)
                {
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("User {Username} logged in successfully", user.UserName);

                var token = _jwtTokenService.GenerateToken(user);

                return Ok(new
                {
                    message = "Login successful",
                    userId = user.Id,
                    username = user.UserName,
                    email = user.Email,
                    psnId = user.PsnId,
                    playstyle = user.Playstyle,
                    token = token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login");
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }


        [HttpGet("profile")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new
                {
                    userId = user.Id,
                    username = user.UserName,
                    email = user.Email,
                    psnId = user.PsnId,
                    playstyle = user.Playstyle,
                    favoriteBuild = user.FavoriteBuild,
                    createdAt = user.CreatedAt,
                    lastLoginAt = user.LastLoginAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }
    }


    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? PsnId { get; set; }
        public string? Playstyle { get; set; }
    }

    public class LoginRequest
    {
        public string EmailOrUsername { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}