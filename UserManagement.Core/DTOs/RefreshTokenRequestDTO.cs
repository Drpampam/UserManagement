using System;
namespace UserManagement.Core.DTOs
{
    public class RefreshTokenRequestDTO
    {
        public string UserId { get; set; }
        public string RefreshToken { get; set; }
    }
}

