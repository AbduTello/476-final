using System;

namespace DriveShare.API.Models.DTOs
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
    }
} 