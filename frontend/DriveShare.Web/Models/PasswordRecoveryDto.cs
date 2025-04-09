using System.ComponentModel.DataAnnotations;

namespace DriveShare.Web.Models
{
    public class PasswordRecoveryDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Answer to security question 1 is required")]
        public string SecurityAnswer1 { get; set; } = string.Empty;

        [Required(ErrorMessage = "Answer to security question 2 is required")]
        public string SecurityAnswer2 { get; set; } = string.Empty;

        [Required(ErrorMessage = "Answer to security question 3 is required")]
        public string SecurityAnswer3 { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        public string NewPassword { get; set; } = string.Empty;
    }
}