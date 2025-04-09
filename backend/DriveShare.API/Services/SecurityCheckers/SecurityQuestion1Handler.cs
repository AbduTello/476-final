using DriveShare.API.Models;
using DriveShare.API.Models.DTOs;
using System;
using System.Threading.Tasks;

namespace DriveShare.API.Services.SecurityCheckers
{
    public class SecurityQuestion1Handler : SecurityQuestionHandlerBase
    {
        protected override Task<bool> CanHandleAsync(ApplicationUser user, PasswordRecoveryDto dto)
        {
            // Compare answer 1 (case-insensitive)
            bool match = string.Equals(user.SecurityAnswer1, dto.SecurityAnswer1, StringComparison.OrdinalIgnoreCase);
            // Simulate async operation if needed, otherwise return completed task
            return Task.FromResult(match);
        }
    }
}