using System;

namespace BACKEND.DOMAIN.Objects
{
    // Server-side record of an issued refresh token. Stored so it can be looked up,
    // rotated, and revoked (logout / reuse detection) instead of trusting the raw
    // token value alone.
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
