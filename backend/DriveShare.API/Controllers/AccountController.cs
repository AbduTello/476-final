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
    
    // Create a new ApplicationUser with all provided details.
    var user = new ApplicationUser
    {
        UserName = dto.Email,
        Email = dto.Email,
        SecurityQuestion1 = dto.SecurityQuestion1,
        SecurityAnswer1 = dto.SecurityAnswer1,
        SecurityQuestion2 = dto.SecurityQuestion2,  // New mapping
        SecurityAnswer2 = dto.SecurityAnswer2,        // New mapping
        SecurityQuestion3 = dto.SecurityQuestion3,    // New mapping
        SecurityAnswer3 = dto.SecurityAnswer3         // New mapping
    };

    // Create the user using the Identity UserManager.
    var result = await _userManager.CreateAsync(user, dto.Password);
    if (result.Succeeded)
    {
        // Optionally, sign the user in automatically (if desired).
        // await _signInManager.SignInAsync(user, isPersistent: false);
        return Ok(new { message = "Registration successful" });
    }

    // If any errors occurred during registration, add them to the ModelState.
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
