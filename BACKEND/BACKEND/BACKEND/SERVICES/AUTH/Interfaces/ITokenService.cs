using BACKEND.DOMAIN.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace BACKEND.SERVICES.AUTH.Interfaces
{
    public interface ITokenService
    {
        // Signed, short-lived JWT carrying the user's identity claims.
        string GenerateAccessToken(User user);

        // Opaque, high-entropy random string. Not a JWT - it's just a lookup
        // key into the RefreshToken table, which is what makes revocation possible.
        string GenerateRefreshTokenValue();

        int AccessTokenMinutes { get; }
        int RefreshTokenDays { get; }
    }
}
