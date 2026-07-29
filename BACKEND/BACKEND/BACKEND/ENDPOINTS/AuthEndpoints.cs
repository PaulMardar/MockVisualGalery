using BACKEND.DOMAIN.DTOS;
using BACKEND.SERVICES.AUTH;
using BACKEND.SERVICES.AUTH.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BACKEND.ENDPOINTS
{
    public static class AuthEndpoints
    {
        public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/auth").WithTags("Auth");

            // POST /api/auth/login -> AuthService.Login
            group.MapPost("/login", (LoginRequest request, IAuthService authService) =>
            {
                var passwordHash = PasswordHasher.Hash(request.Password);  // maybe already hashed ??? to decide later
                var result = authService.Login(request.Email, passwordHash);
                return result is null
                    ? Results.Unauthorized()
                    : Results.Ok(result);
            })
            .WithName("Login")
            .WithSummary("Exchanges email + password for an access token and a refresh token")
            .AllowAnonymous()    // this allows the turn on off auth
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

            // POST /api/auth/refresh -> AuthService.Refresh
            group.MapPost("/refresh", (RefreshTokenRequest request, IAuthService authService) =>
            {
                var result = authService.Refresh(request.RefreshToken);
                return result is null
                    ? Results.Unauthorized()
                    : Results.Ok(result);
            })
            .WithName("RefreshToken")
            .WithSummary("Exchanges a valid refresh token for a new access/refresh token pair")
            .AllowAnonymous()
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

            // POST /api/auth/logout -> AuthService.Logout
            group.MapPost("/logout", (RefreshTokenRequest request, IAuthService authService) =>
            {
                var success = authService.Logout(request.RefreshToken);
                return success ? Results.NoContent() : Results.NotFound();
            })
            .WithName("Logout")
            .WithSummary("Revokes a refresh token")
            .AllowAnonymous()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            return app;
        }
    }
}
