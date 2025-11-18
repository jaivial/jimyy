using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowAutomation.Core.DTOs;
using WorkflowAutomation.Core.Interfaces;

namespace WorkflowAutomation.API.Controllers
{
    /// <summary>
    /// Controller for authentication and authorization operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user account
        /// </summary>
        /// <param name="request">Registration request with user details</param>
        /// <returns>Authentication response with JWT token</returns>
        /// <response code="200">User registered successfully</response>
        /// <response code="400">Invalid request or validation failed</response>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                request.Role
            );

            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            var response = new AuthResponse
            {
                Token = result.Token,
                RefreshToken = result.RefreshToken,
                ExpiresAt = result.ExpiresAt,
                User = result.User!
            };

            _logger.LogInformation("User registered successfully: {Email}", request.Email);
            return Ok(response);
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>Authentication response with JWT token</returns>
        /// <response code="200">Login successful</response>
        /// <response code="400">Invalid credentials</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(request.Email, request.Password);

            if (!result.Success)
            {
                return Unauthorized(new { message = result.ErrorMessage });
            }

            var response = new AuthResponse
            {
                Token = result.Token,
                RefreshToken = result.RefreshToken,
                ExpiresAt = result.ExpiresAt,
                User = result.User!
            };

            _logger.LogInformation("User logged in successfully: {Email}", request.Email);
            return Ok(response);
        }

        /// <summary>
        /// Refresh an expired JWT token using a refresh token
        /// </summary>
        /// <param name="request">Refresh token request</param>
        /// <returns>New JWT token and refresh token</returns>
        /// <response code="200">Token refreshed successfully</response>
        /// <response code="400">Invalid or expired refresh token</response>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RefreshTokenAsync(request.RefreshToken);

            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            var response = new AuthResponse
            {
                Token = result.Token,
                RefreshToken = result.RefreshToken,
                ExpiresAt = result.ExpiresAt,
                User = result.User!
            };

            return Ok(response);
        }

        /// <summary>
        /// Logout by revoking the refresh token
        /// </summary>
        /// <param name="request">Refresh token to revoke</param>
        /// <returns>Success status</returns>
        /// <response code="200">Logout successful</response>
        /// <response code="400">Invalid refresh token</response>
        [HttpPost("logout")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _authService.RevokeTokenAsync(request.RefreshToken);

            if (!success)
            {
                return BadRequest(new { message = "Invalid refresh token" });
            }

            _logger.LogInformation("User logged out successfully");
            return Ok(new { message = "Logged out successfully" });
        }

        /// <summary>
        /// Revoke a specific refresh token (Admin only)
        /// </summary>
        /// <param name="request">Refresh token to revoke</param>
        /// <returns>Success status</returns>
        /// <response code="200">Token revoked successfully</response>
        /// <response code="400">Invalid refresh token</response>
        /// <response code="403">Forbidden - Admin role required</response>
        [HttpPost("revoke")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _authService.RevokeTokenAsync(request.RefreshToken);

            if (!success)
            {
                return BadRequest(new { message = "Invalid refresh token" });
            }

            _logger.LogInformation("Refresh token revoked by admin");
            return Ok(new { message = "Token revoked successfully" });
        }

        /// <summary>
        /// Get current authenticated user information
        /// </summary>
        /// <returns>Current user information</returns>
        /// <response code="200">User information retrieved successfully</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var firstName = User.FindFirst("firstName")?.Value;
            var lastName = User.FindFirst("lastName")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var userDto = new UserDto
            {
                Id = Guid.Parse(userId),
                Email = email ?? string.Empty,
                FirstName = firstName ?? string.Empty,
                LastName = lastName ?? string.Empty,
                Role = Enum.Parse<WorkflowAutomation.Core.Enums.UserRole>(role ?? "Developer"),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            return Ok(userDto);
        }
    }
}
