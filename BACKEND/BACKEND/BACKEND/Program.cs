using BACKEND.CONFIGURATION;
using BACKEND.DOMAIN.Objects;
using BACKEND.ENDPOINTS;
using BACKEND.REPOSITORY;
using BACKEND.SERVICES;
using BACKEND.SERVICES.AUTH;
using BACKEND.SERVICES.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
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

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.Configure<JWTOptions>(builder.Configuration.GetSection("Jwt"));
            builder.Services.Configure<S3Options>(builder.Configuration.GetSection("S3"));

            var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JWTOptions>();
            var authOptions = builder.Configuration.GetSection("Auth").Get<AuthOptions>();

            Console.WriteLine($"[DEBUG] SigningKey length = {jwtOptions.SigningKey?.Length}");

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

            builder.Services.AddSingleton<IRepository<User>>(new InFileRepository<User>(Path.Combine("D:/PROJECT_TESTARE/MockVisualGalery/BACKEND/BACKEND/BACKEND/FILES")));

            builder.Services.AddSingleton<IRepository<Photo>>(new InFileRepository<Photo>(Path.Combine("D:/PROJECT_TESTARE/MockVisualGalery/BACKEND/BACKEND/BACKEND/FILES")));

            builder.Services.AddSingleton<IRepository<RefreshToken>>(new InFileRepository<RefreshToken>(Path.Combine("D:/PROJECT_TESTARE/MockVisualGalery/BACKEND/BACKEND/BACKEND/FILES")));

            // Domain services
            var filesRoot = "D:/PROJECT_TESTARE/MockVisualGalery/BACKEND/BACKEND/BACKEND/FILES";

            // --- Photo file storage: always local disk, optionally also synced to S3 ---
            // "S3:Enabled" in appsettings.json is the single on/off switch. Local
            // disk is always written to and is always the read path; when the
            // switch is true, every save/delete is additionally mirrored to the
            // configured S3 bucket.
            bool s3SyncEnabled = builder.Configuration.GetValue<bool>("S3:Enabled");

            var photosDir = Path.Combine(filesRoot, "Photos");

            builder.Services.AddSingleton<IPhotoFileStorage>(sp =>
            {
                var local = new PhotoFileStorage(photosDir);

                if (!s3SyncEnabled)
                    return local;

                var s3Options = sp.GetRequiredService<IOptions<S3Options>>();
                var s3 = new S3PhotoFileStorage(s3Options);
                return new SyncedPhotoFileStorage(local, s3);
            });

            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IPhotoService, PhotoService>();
            builder.Services.AddSingleton<ITokenService, TokenService>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            var app = builder.Build();

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