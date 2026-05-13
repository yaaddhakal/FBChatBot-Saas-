
using System;

namespace CoreCommon.AuthModel.RefreshToken
{
    /// <summary>
    /// Enhanced token response model
    /// </summary>
    public class TokenResponseModel
    {
        /// <summary>
        /// JWT Access Token
        /// </summary>
        public string Token { get; set; }= string.Empty;

        /// <summary>
        /// Refresh token for getting new access tokens
        /// </summary>
        public string RefreshToken { get; set; }=string.Empty;

        /// <summary>
        /// User ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// User's role name
        /// </summary>
        public string RoleName { get; set; } = string.Empty;

        /// <summary>
        /// Token expiration time in seconds
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Token type (usually "Bearer")
        /// </summary>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// Timestamp when token was issued
        /// </summary>
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    }
}

