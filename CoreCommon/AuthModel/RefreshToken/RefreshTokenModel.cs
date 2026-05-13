using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreCommon.AuthModel.RefreshToken
{
    public class RefreshTokenModel
    {
        public int TokenId { get; set; }   // maps to PK
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }



    }
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
        public string accessToken { get; set; } = string.Empty;
    }
    public class RefreshRequestDto
    {
        public string RefreshToken { get; set; }=string.Empty;
    }

}