using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DriveShare.API.Models;
using DriveShare.API.Models.DTOs;

namespace DriveShare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, 
                                 SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterDto dto)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }
    
    // Hard-code the questions and use the user's provided answers.
    var user = new ApplicationUser
    {
        UserName = dto.Email,
        Email = dto.Email,
        SecurityQuestion1 = "What is your favorite color?",
        SecurityAnswer1 = dto.SecurityAnswer1,
        SecurityQuestion2 = "What is your pet's name?",
        SecurityAnswer2 = dto.SecurityAnswer2,
        SecurityQuestion3 = "What city were you born in?",
        SecurityAnswer3 = dto.SecurityAnswer3
    };

    var result = await _userManager.CreateAsync(user, dto.Password);
    if (result.Succeeded)
    {
        return Ok(new { message = "Registration successful" });
    }

    foreach (var error in result.Errors)
    {
        ModelState.AddModelError(string.Empty, error.Description);
    }
    return BadRequest(ModelState);
}


        // Optional: Implement a login endpoint for authentication.
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
                return Ok(new { message = "Login successful" });
            }
            
            return Unauthorized("Invalid login attempt");
        }
    }
}
