namespace WorkflowAutomation.Core.DTOs
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserDto? User { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public static AuthResult SuccessResult(string token, string refreshToken, DateTime expiresAt, UserDto user)
        {
            return new AuthResult
            {
                Success = true,
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                User = user
            };
        }

        public static AuthResult FailureResult(string errorMessage)
        {
            return new AuthResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
