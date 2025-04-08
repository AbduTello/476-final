using Microsoft.AspNetCore.Identity;

namespace DriveShare.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Add custom properties for security questions
        public string? SecurityQuestion1 { get; set; }
        public string? SecurityAnswer1 { get; set; }

        public string? SecurityQuestion2 { get; set; }
        public string? SecurityAnswer2 { get; set; }

        public string? SecurityQuestion3 { get; set; }
        public string? SecurityAnswer3 { get; set; }
        // You can add SecurityQuestion2, SecurityAnswer2, etc. as needed.
    }
}
