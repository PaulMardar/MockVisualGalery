using BACKEND.DOMAIN;
using BACKEND.DOMAIN.DTOS;
using BACKEND.DOMAIN.Objects;
using BACKEND.SERVICES.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BACKEND.ENDPOINTS
{
    public static class UserEndpoints
    {
        public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/users").WithTags("Users");

            // POST /api/users/register  -> UserService.Register
            group.MapPost("/register", (RegisterUserRequest request, IUserService userService) =>
            {
                try
                {
                    var passwordHash = HashPassword(request.Password);
                    var user = userService.Register(request.Name, request.Email, passwordHash);
                    return Results.Created($"/api/users/{user.Id}", ToDto(user));
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.Conflict(new { error = ex.Message });
                }
            })
            .WithName("RegisterUser")
            .WithSummary("Registers a new user")
            .Produces<UserDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

            // POST /api/users -> UserService.Add (raw passthrough, no validation/hashing)
            group.MapPost("/", (CreateUserRequest request, IUserService userService) =>
            {
                var user = new User
                {
                    Name = request.Name,
                    Email = request.Email,
                    PasswordHash = request.PasswordHash
                };
                var created = userService.Add(user);
                return Results.Created($"/api/users/{created.Id}", ToDto(created));
            })
            .WithName("AddUser")
            .WithSummary("Adds a user directly, bypassing Register's validation")
            .Produces<UserDto>(StatusCodes.Status201Created);

            // GET /api/users -> UserService.GetAll
            group.MapGet("/", (IUserService userService) =>
            {
                var users = userService.GetAll().Select(ToDto);
                return Results.Ok(users);
            })
            .WithName("GetAllUsers")
            .WithSummary("Returns every registered user")
            .Produces<IEnumerable<UserDto>>(StatusCodes.Status200OK);

            // GET /api/users/{id} -> UserService.GetById
            group.MapGet("/{id:int}", (int id, IUserService userService) =>
            {
                var user = userService.GetById(id);
                return user is null ? Results.NotFound() : Results.Ok(ToDto(user));
            })
            .WithName("GetUserById")
            .WithSummary("Returns a single user by id")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            // DELETE /api/users/{id} -> UserService.Delete
            group.MapDelete("/{id:int}", (int id, IUserService userService) =>
            {
                var deleted = userService.Delete(id);
                return deleted ? Results.NoContent() : Results.NotFound();
            })
            .WithName("DeleteUser")
            .WithSummary("Deletes a user by id")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

            return app;
        }

        private static UserDto ToDto(User user) => new()
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };

        // Simple placeholder hashing so the endpoint has something to pass as
        // "passwordHash". Swap this for a real algorithm (e.g. BCrypt.Net,
        // ASP.NET Core Identity's PasswordHasher<T>) before shipping to production.
        private static string HashPassword(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}