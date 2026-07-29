using System;
using System.Collections.Generic;
using System.Text;

namespace BACKEND.CONFIGURATION
{
    public class JWTOptions
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string SigningKey { get; set; } = string.Empty;
        public int AccessTokenMinutes { get; set; } = 120;
        public int RefreshTokenDays { get; set; } = 30;
    }

    // Bound from the "Auth" section of appsettings.json.
    // This is the single switch that turns the whole JWT requirement on/off.
    public class AuthOptions
    {
        public bool Enabled { get; set; } = true;
    }
}
