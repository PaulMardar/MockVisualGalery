using BACKEND.DOMAIN;
using BACKEND.SERVICES.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace BACKEND.SERVICES
{
    public class UserService : IUserService
    {
        public User Add(User entity)
        {
            throw new NotImplementedException();
        }

        public bool Delete(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<User> GetAll()
        {
            throw new NotImplementedException();
        }

        public User? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public User Register(string name, string email, string passwordHash)
        {
            throw new NotImplementedException();
        }
    }
}
