using System.ComponentModel.DataAnnotations;

namespace DriveShare.Web.Models;

public class RegisterDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string SecurityQuestion1 { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Security answer is required")]
    public string SecurityAnswer1 { get; set; } = string.Empty;

    [Required]
    public string SecurityQuestion2 { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Security answer is required")]
    public string SecurityAnswer2 { get; set; } = string.Empty;

    [Required]
    public string SecurityQuestion3 { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Security answer is required")]
    public string SecurityAnswer3 { get; set; } = string.Empty;
} 