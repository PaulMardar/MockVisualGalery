using System;

namespace BACKEND.DOMAIN.Objects
{
    public class RefreshToken : IEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool Revoked { get; set; } = false;
    }
}
