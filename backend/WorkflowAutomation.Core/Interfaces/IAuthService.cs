using WorkflowAutomation.Core.DTOs;
using WorkflowAutomation.Core.Enums;

namespace WorkflowAutomation.Core.Interfaces
{
    /// <summary>
    /// Service for handling authentication operations
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user with the provided credentials
        /// </summary>
        /// <param name="email">User email address</param>
        /// <param name="password">User password (will be hashed)</param>
        /// <param name="firstName">User first name</param>
        /// <param name="lastName">User last name</param>
        /// <param name="role">User role (Admin, Developer, Viewer)</param>
        /// <returns>Authentication result with JWT token if successful</returns>
        Task<AuthResult> RegisterAsync(string email, string password, string firstName, string lastName, UserRole role);

        /// <summary>
        /// Authenticates a user with email and password
        /// </summary>
        /// <param name="email">User email address</param>
        /// <param name="password">User password</param>
        /// <returns>Authentication result with JWT token if successful</returns>
        Task<AuthResult> LoginAsync(string email, string password);

        /// <summary>
        /// Refreshes an expired JWT token using a valid refresh token
        /// </summary>
        /// <param name="refreshToken">The refresh token</param>
        /// <returns>Authentication result with new JWT token if successful</returns>
        Task<AuthResult> RefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Revokes a refresh token to prevent future use
        /// </summary>
        /// <param name="refreshToken">The refresh token to revoke</param>
        /// <returns>True if successfully revoked, false otherwise</returns>
        Task<bool> RevokeTokenAsync(string refreshToken);
    }
}
