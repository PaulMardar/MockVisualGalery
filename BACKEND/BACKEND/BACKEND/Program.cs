using BACKEND.CONFIGURATION;
using BACKEND.DOMAIN.Objects;
using BACKEND.ENDPOINTS;
using BACKEND.REPOSITORY;
using BACKEND.SERVICES;
using BACKEND.SERVICES.AUTH;
using BACKEND.SERVICES.AUTH.Interfaces;
using BACKEND.SERVICES.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IO;
using System.Text;

namespace BACKEND
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- Services ---------------------------------------------------------
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // --- Auth configuration --------------------------------------------------
            // "Auth:Enabled" in appsettings.json is the single on/off switch:
            // - true  -> every endpoint except /api/auth/* and /api/users/register
            //            requires a valid Bearer access token.
            // - false -> auth middleware is still wired up (so /api/auth/login still
            //            works and issues real tokens), but RequireAuthorization()
            //            is never applied to the other endpoint groups, so nothing
            //            is enforced. Handy for local development.
            builder.Services.Configure<JWTOptions>(builder.Configuration.GetSection("Jwt"));

            var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JWTOptions>() ?? new JWTOptions();
            var authOptions = builder.Configuration.GetSection("Auth").Get<AuthOptions>() ?? new AuthOptions();

            Console.WriteLine($"[DEBUG] SigningKey length = {jwtOptions.SigningKey?.Length ?? -1}");

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtOptions.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Services.AddAuthorization();

            // --- Repositories -------------------------------------------------------
            // builder.Services.AddSingleton<IRepository<User>, MemoryRepository<User>>();
            // builder.Services.AddSingleton<IRepository<Photo>, MemoryRepository<Photo>>();

            builder.Services.AddSingleton<IRepository<User>>(
                new InFileRepository<User>(Path.Combine("D:/PROJECT_TESTARE/MockVisualGalery/BACKEND/BACKEND/BACKEND/FILES")));

            builder.Services.AddSingleton<IRepository<Photo>>(
                new InFileRepository<Photo>(Path.Combine("D:/PROJECT_TESTARE/MockVisualGalery/BACKEND/BACKEND/BACKEND/FILES")));

            builder.Services.AddSingleton<IRepository<RefreshToken>>(
                new InFileRepository<RefreshToken>(Path.Combine("D:/PROJECT_TESTARE/MockVisualGalery/BACKEND/BACKEND/BACKEND/FILES")));

            // Domain services

            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IPhotoService, PhotoService>();
            builder.Services.AddSingleton<ITokenService, TokenService>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            var app = builder.Build();

            // --- Seed data ---------------------------------------------------------
            // Uses app.Services (the container the app itself runs on) instead of a
            // separate BuildServiceProvider() call, which would spin up a second,
            // never-disposed DI container (ASP0000).
            // Guarded with GetByEmail because InFileRepository persists to disk -
            // without this check, Add() throws "id already exists" on every restart
            // after the first one.
            var userRepository = app.Services.GetRequiredService<IRepository<User>>();

            // --- Middleware ---------------------------------------------------------
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            // --- Endpoints ---------------------------------------------------------
            app.MapAuthEndpoints();
            app.MapUserEndpoints(authOptions.Enabled);
            app.MapPhotoEndpoints(authOptions.Enabled);

            app.Run();

        }
    }
}