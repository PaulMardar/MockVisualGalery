using BACKEND.DOMAIN;
using BACKEND.DOMAIN.Objects;
using BACKEND.SERVICES;
using BACKEND.SERVICES.AUTH;
using BACKEND.SERVICES.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BACKEND.ENDPOINTS
{
    public static class UserEndpoints
    {
        // authEnabled comes from the "Auth:Enabled" config switch (see Program.cs).
        // When true, every route in this group requires a valid access token
        // except /register, which has to stay reachable so new accounts can be
        // created in the first place.
        public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app, bool authEnabled)
        {
            var group = app.MapGroup("/api/users").WithTags("Users");

            if (authEnabled)
                group.RequireAuthorization();

            // POST /api/users/register  -> UserService.Register
            group.MapPost("/register", (RegisterUserRequest request, IUserService userService) =>
            {
                try
                {
                    var passwordHash = PasswordHasher.Hash(request.Password);
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
            .AllowAnonymous()
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
    }
}