using BACKEND.DOMAIN.DTOS;
using System;
using System.Collections.Generic;
using System.Text;

namespace BACKEND.SERVICES.AUTH.Interfaces
{
    internal interface IAuthService
    {
        AuthResponse? Login(string email, string passwordHash);

        AuthResponse? Refresh(string refreshToken);
        bool Logout(string refreshToken);
    }
}
