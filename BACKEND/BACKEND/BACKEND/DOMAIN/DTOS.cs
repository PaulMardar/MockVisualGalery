using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
namespace BACKEND.DOMAIN
{
    public class PhotoDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public int Length { get; set; }
        public int Width { get; set; }
        public List<string> Tags { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public int OwnerId { get; set; }
    }

    public class UploadPhotoForm
    {
        public IFormFile File { get; set; } = default!;
        public int Length { get; set; }
        public int Width { get; set; }
        public string? Tags { get; set; } // comma-separated, e.g. "party,2025,night"
    }

    public class TagRequest
    {
        public string Tag { get; set; } = string.Empty;
    }

    public class CreatePhotoRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public int Length { get; set; }
        public int Width { get; set; }
        public List<string> Tags { get; set; } = new();
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public int OwnerId { get; set; } = -1;
    }
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; } = DateTime.Now.AddHours(2); // Set default expiration to 2 hours from now
    }
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class RegisterUserRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class CreateUserRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }
}
