using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using WorkflowAutomation.Core.DTOs;
using WorkflowAutomation.Core.Entities;
using WorkflowAutomation.Core.Enums;
using WorkflowAutomation.Core.Helpers;
using WorkflowAutomation.Core.Interfaces;
using WorkflowAutomation.Infrastructure.Data;

namespace WorkflowAutomation.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            ApplicationDbContext dbContext,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResult> RegisterAsync(
            string email,
            string password,
            string firstName,
            string lastName,
            UserRole role)
        {
            try
            {
                // Validate password strength
                var (isValid, errorMessage) = PasswordValidator.Validate(password);
                if (!isValid)
                {
                    return AuthResult.FailureResult(errorMessage);
                }

                // Check if user already exists
                if (await _userRepository.EmailExistsAsync(email))
                {
                    return AuthResult.FailureResult("Email address is already registered");
                }

                // Hash password using BCrypt
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

                // Create new user
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email.ToLowerInvariant(),
                    PasswordHash = passwordHash,
                    FirstName = firstName,
                    LastName = lastName,
                    Role = role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _userRepository.AddAsync(user);

                _logger.LogInformation("User registered successfully: {Email}", email);

                // Generate tokens
                var token = GenerateJwtToken(user);
                var refreshToken = await GenerateAndSaveRefreshTokenAsync(user.Id);
                var expiresAt = DateTime.UtcNow.AddMinutes(GetJwtExpirationMinutes());

                var userDto = MapToUserDto(user);
                return AuthResult.SuccessResult(token, refreshToken, expiresAt, userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration for email: {Email}", email);
                return AuthResult.FailureResult("An error occurred during registration");
            }
        }

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email.ToLowerInvariant());

                if (user == null)
                {
                    _logger.LogWarning("Login failed: User not found for email: {Email}", email);
                    return AuthResult.FailureResult("Invalid email or password");
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Login failed: User account is inactive: {Email}", email);
                    return AuthResult.FailureResult("User account is inactive");
                }

                // Verify password using BCrypt
                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    _logger.LogWarning("Login failed: Invalid password for email: {Email}", email);
                    return AuthResult.FailureResult("Invalid email or password");
                }

                _logger.LogInformation("User logged in successfully: {Email}", email);

                // Generate tokens
                var token = GenerateJwtToken(user);
                var refreshToken = await GenerateAndSaveRefreshTokenAsync(user.Id);
                var expiresAt = DateTime.UtcNow.AddMinutes(GetJwtExpirationMinutes());

                var userDto = MapToUserDto(user);
                return AuthResult.SuccessResult(token, refreshToken, expiresAt, userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", email);
                return AuthResult.FailureResult("An error occurred during login");
            }
        }

        public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var storedToken = _dbContext.RefreshTokens
                    .FirstOrDefault(rt => rt.Token == refreshToken);

                if (storedToken == null)
                {
                    _logger.LogWarning("Refresh token not found");
                    return AuthResult.FailureResult("Invalid refresh token");
                }

                if (storedToken.IsRevoked)
                {
                    _logger.LogWarning("Refresh token is revoked: {TokenId}", storedToken.Id);
                    return AuthResult.FailureResult("Refresh token has been revoked");
                }

                if (storedToken.ExpiresAt < DateTime.UtcNow)
                {
                    _logger.LogWarning("Refresh token expired: {TokenId}", storedToken.Id);
                    return AuthResult.FailureResult("Refresh token has expired");
                }

                var user = await _userRepository.GetByIdAsync(storedToken.UserId);
                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning("User not found or inactive for refresh token: {UserId}", storedToken.UserId);
                    return AuthResult.FailureResult("Invalid user");
                }

                // Revoke old refresh token
                storedToken.IsRevoked = true;
                storedToken.RevokedAt = DateTime.UtcNow;

                // Generate new tokens
                var newToken = GenerateJwtToken(user);
                var newRefreshToken = await GenerateAndSaveRefreshTokenAsync(user.Id);
                var expiresAt = DateTime.UtcNow.AddMinutes(GetJwtExpirationMinutes());

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Token refreshed successfully for user: {UserId}", user.Id);

                var userDto = MapToUserDto(user);
                return AuthResult.SuccessResult(newToken, newRefreshToken, expiresAt, userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return AuthResult.FailureResult("An error occurred during token refresh");
            }
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            try
            {
                var storedToken = _dbContext.RefreshTokens
                    .FirstOrDefault(rt => rt.Token == refreshToken);

                if (storedToken == null)
                {
                    return false;
                }

                storedToken.IsRevoked = true;
                storedToken.RevokedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Refresh token revoked: {TokenId}", storedToken.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking refresh token");
                return false;
            }
        }

        private string GenerateJwtToken(User user)
        {
            var secret = _configuration["JWT:Secret"]
                ?? throw new InvalidOperationException("JWT Secret is not configured");

            var issuer = _configuration["JWT:Issuer"] ?? "WorkflowAutomation";
            var audience = _configuration["JWT:Audience"] ?? "WorkflowAutomation";
            var expirationMinutes = GetJwtExpirationMinutes();

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("firstName", user.FirstName),
                new Claim("lastName", user.LastName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(Guid userId)
        {
            var refreshToken = GenerateRefreshToken();
            var refreshTokenExpirationDays = GetRefreshTokenExpirationDays();

            var token = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpirationDays),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            _dbContext.RefreshTokens.Add(token);
            await _dbContext.SaveChangesAsync();

            return refreshToken;
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private int GetJwtExpirationMinutes()
        {
            return int.TryParse(_configuration["JWT:ExpirationMinutes"], out var minutes)
                ? minutes
                : 60; // Default 60 minutes
        }

        private int GetRefreshTokenExpirationDays()
        {
            return int.TryParse(_configuration["JWT:RefreshTokenExpirationDays"], out var days)
                ? days
                : 7; // Default 7 days
        }

        private static UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }
    }
}
