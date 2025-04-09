using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DriveShare.API.Models;
using DriveShare.API.Models.DTOs;
using DriveShare.API.Services;
using Microsoft.AspNetCore.Authorization;
using DriveShare.API.Services.SecurityCheckers;

namespace DriveShare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtService _jwtService;
        private readonly SessionManager _sessionManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = new JwtService(
                configuration["Jwt:Key"] ?? throw new Exception("JWT Key not found"),
                configuration["Jwt:Issuer"] ?? "DriveShare",
                configuration["Jwt:Audience"] ?? "DriveShareUsers"
            );
            _sessionManager = SessionManager.Instance;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                SecurityQuestion1 = "What is your mother's maiden name?",
                SecurityAnswer1 = dto.SecurityAnswer1,
                SecurityQuestion2 = "What was the name of your first pet?",
                SecurityAnswer2 = dto.SecurityAnswer2,
                SecurityQuestion3 = "In what city were you born?",
                SecurityAnswer3 = dto.SecurityAnswer3
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (result.Succeeded)
            {
                var token = _jwtService.GenerateToken(user);
                _sessionManager.AddSession(user.Id, token);
                
                return Ok(new LoginResponseDto
                {
                    Token = token,
                    Email = user.Email ?? string.Empty,
                    UserId = user.Id,
                    Expiration = DateTime.Now.AddHours(24)
                });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return BadRequest(ModelState);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var result = await _signInManager.PasswordSignInAsync(dto.Email, dto.Password, isPersistent: false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                {
                    return Unauthorized("Invalid login attempt");
                }

                var token = _jwtService.GenerateToken(user);
                _sessionManager.AddSession(user.Id, token);

                return Ok(new LoginResponseDto
                {
                    Token = token,
                    Email = user.Email ?? string.Empty,
                    UserId = user.Id,
                    Expiration = DateTime.Now.AddHours(24)
                });
            }
            
            return Unauthorized("Invalid login attempt");
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst("sub")?.Value;
            if (userId != null)
            {
                _sessionManager.RemoveSession(userId);
            }
            
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logged out successfully" });
        }

        [Authorize]
        [HttpGet("session-info")]
        public IActionResult GetSessionInfo()
        {
            var userId = User.FindFirst("sub")?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var isActive = _sessionManager.IsSessionActive(userId);
            if (!isActive)
            {
                return Unauthorized("Session expired");
            }

            return Ok(new
            {
                ActiveSessions = _sessionManager.GetActiveSessionCount(),
                LastActivity = DateTime.UtcNow
            });
        }

        [HttpPost("recover")]
        public async Task<IActionResult> RecoverPassword([FromBody] PasswordRecoveryDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Create the chain of security question handlers
            var handler1 = new SecurityQuestion1Handler();
            var handler2 = new SecurityQuestion2Handler();
            var handler3 = new SecurityQuestion3Handler();

            // Set up the chain
            handler1.NextHandler = handler2;
            handler2.NextHandler = handler3;

            // Start the chain
            var isValid = await handler1.HandleAsync(user, dto);
            if (!isValid)
            {
                return BadRequest("Security answers do not match");
            }

            // Reset the password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
            
            if (result.Succeeded)
            {
                return Ok(new { message = "Password has been reset successfully" });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return BadRequest(ModelState);
        }
    }
}
