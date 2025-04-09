using System.ComponentModel.DataAnnotations;

namespace DriveShare.API.Models.DTOs
{
    public class PasswordRecoveryDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string SecurityAnswer1 { get; set; }

        [Required]
        public required string SecurityAnswer2 { get; set; }

        [Required]
        public required string SecurityAnswer3 { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public required string NewPassword { get; set; }
    }
}