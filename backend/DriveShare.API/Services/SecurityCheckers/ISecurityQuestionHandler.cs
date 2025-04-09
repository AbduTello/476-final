using DriveShare.API.Models;
using DriveShare.API.Models.DTOs;
using System.Threading.Tasks;

namespace DriveShare.API.Services.SecurityCheckers
{
    // Interface for security check handlers
    public interface ISecurityQuestionHandler
    {
        ISecurityQuestionHandler? NextHandler { get; set; }
        Task<bool> HandleAsync(ApplicationUser user, PasswordRecoveryDto dto);
    }

    // Abstract base class for common functionality
    public abstract class SecurityQuestionHandlerBase : ISecurityQuestionHandler
    {
        public ISecurityQuestionHandler? NextHandler { get; set; }

        public virtual async Task<bool> HandleAsync(ApplicationUser user, PasswordRecoveryDto dto)
        {
            // Check if the current handler can validate
            if (await CanHandleAsync(user, dto))
            {
                // If there's a next handler, pass it along
                if (NextHandler != null)
                {
                    return await NextHandler.HandleAsync(user, dto);
                }
                // This is the last handler and it succeeded
                return true;
            }
            // Current handler failed
            return false;
        }

        // Method specific handlers must implement to perform their check
        protected abstract Task<bool> CanHandleAsync(ApplicationUser user, PasswordRecoveryDto dto);
    }
}