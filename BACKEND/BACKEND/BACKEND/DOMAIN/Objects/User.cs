using System;
using System.Collections.Generic;
using System.Text;

namespace BACKEND.DOMAIN.Objects
{
    public class User : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public User() { }

        public User(int id, string name, string email, string passwordHash, DateTime createdAt)
        {
            Id = id;
            Name = name;
            Email = email;
            PasswordHash = passwordHash;
            CreatedAt = createdAt;
        }
    }
}
