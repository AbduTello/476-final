namespace DriveShare.API.Models.DTOs
{
    public class RegisterDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        // Include security questions and answers:
        public required string SecurityQuestion1 { get; set; }
        public required string SecurityAnswer1 { get; set; }
        public required string SecurityQuestion2 { get; set; }
        public required string SecurityAnswer2 { get; set; }
        public required string SecurityQuestion3 { get; set; }
        public required string SecurityAnswer3 { get; set; }
        // You can add additional questions/answers as needed.
    }
}
