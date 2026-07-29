using BACKEND.DOMAIN.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace BACKEND.SERVICES.Interfaces
{
    public interface IUserService : IService<User>
    {
        User Register(string name, string email, string passwordHash);
        User? GetByEmail(string email);
    }
}
