using DriveShare.API.Models;
using DriveShare.API.Models.DTOs;
using System;
using System.Threading.Tasks;

namespace DriveShare.API.Services.SecurityCheckers
{
    public class SecurityQuestion2Handler : SecurityQuestionHandlerBase
    {
        protected override Task<bool> CanHandleAsync(ApplicationUser user, PasswordRecoveryDto dto)
        {
            // Compare answer 2 (case-insensitive)
            bool match = string.Equals(user.SecurityAnswer2, dto.SecurityAnswer2, StringComparison.OrdinalIgnoreCase);
            return Task.FromResult(match);
        }
    }
}