using DriveShare.API.Models;
using DriveShare.API.Models.DTOs;
using System;
using System.Threading.Tasks;

namespace DriveShare.API.Services.SecurityCheckers
{
    public class SecurityQuestion3Handler : SecurityQuestionHandlerBase
    {
        protected override Task<bool> CanHandleAsync(ApplicationUser user, PasswordRecoveryDto dto)
        {
            // Compare answer 3 (case-insensitive)
            bool match = string.Equals(user.SecurityAnswer3, dto.SecurityAnswer3, StringComparison.OrdinalIgnoreCase);
            return Task.FromResult(match);
        }
    }
}