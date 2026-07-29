using BACKEND.DOMAIN;
using BACKEND.REPOSITORY;
using BACKEND.SERVICES.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace BACKEND.SERVICES
{
    public class UserService : IUserService
    {
        private IRepository<User> _userRepository;

        public UserService(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public User Register(string name, string email, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required.", nameof(name));

            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                throw new ArgumentException("A valid email is required.", nameof(email));

            if (_userRepository.GetAll().Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"A user with email '{email}' already exists.");

            var user = new User { Name = name, Email = email, PasswordHash = passwordHash};

            return Add(user);
        }
        public User Add(User entity) => _userRepository.Add(entity);

        public User? GetById(int id) => _userRepository.GetById(id);

        public IEnumerable<User> GetAll() => _userRepository.GetAll();
        public bool Delete(int id) => _userRepository.Delete(id);
    }
}
