using BACKEND.DOMAIN;
using BACKEND.DOMAIN.Objects;
using BACKEND.REPOSITORY;
using BACKEND.SERVICES.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace BACKEND.SERVICES.AUTH
{
    public interface IAuthService
    {
        AuthResponse? Login(string email, string passwordHash);

        AuthResponse? Refresh(string refreshToken);
        bool Logout(string refreshToken);
    }
    public class PasswordHasher
    {
        public static string Hash(string password)
        {
            // TODO: Hash password with salt and return the hashed password
            return password;
        }
    }
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly IRepository<RefreshToken> _refreshTokenRepository;
        private readonly ITokenService _tokenService;

        public AuthService(
            IUserService userService,
            IRepository<RefreshToken> refreshTokenRepository,
            ITokenService tokenService)
        {
            _userService = userService;
            _refreshTokenRepository = refreshTokenRepository;
            _tokenService = tokenService;
        }

        public AuthResponse? Login(string email, string passwordHash)
        {
            var user = _userService.GetByEmail(email);
            if (user is null || !user.PasswordHash.Equals(passwordHash, StringComparison.Ordinal))
                return null;

            return IssueTokens(user);
        }

        public AuthResponse? Refresh(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return null;

            var stored = _refreshTokenRepository.GetAll()
                .FirstOrDefault(t => t.Token == refreshToken);

            if (stored is null || stored.Revoked || stored.ExpiresAt < DateTime.UtcNow)
                return null;

            var user = _userService.GetById(stored.UserId);
            if (user is null)
                return null;

            // Rotate: the presented refresh token is single-use. Revoking it here
            // means a stolen-and-replayed token stops working the moment the
            // legitimate client refreshes, which is what makes reuse detectable.
            stored.Revoked = true;
            _refreshTokenRepository.Update(stored);

            return IssueTokens(user);
        }

        public bool Logout(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return false;

            var stored = _refreshTokenRepository.GetAll()
                .FirstOrDefault(t => t.Token == refreshToken);

            if (stored is null || stored.Revoked)
                return false;

            stored.Revoked = true;
            _refreshTokenRepository.Update(stored);
            return true;
        }

        private AuthResponse IssueTokens(User user)
        {
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshTokenValue = _tokenService.GenerateRefreshTokenValue();

            var refreshToken = new RefreshToken
            {
                Token = refreshTokenValue,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(_tokenService.RefreshTokenDays)
            };
            _refreshTokenRepository.Add(refreshToken);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_tokenService.AccessTokenMinutes)
            };
        }
    }
}
