using BACKEND.DOMAIN.Objects;
using BACKEND.ENDPOINTS;
using BACKEND.REPOSITORY;
using BACKEND.SERVICES;
using BACKEND.SERVICES.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

            builder.Services.AddSingleton<IRepository<User>, MemoryRepository<User>>();
            builder.Services.AddSingleton<IRepository<Photo>, MemoryRepository<Photo>>();

            // Domain services

            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IPhotoService, PhotoService>();

            var ServiceProvider = builder.Services.BuildServiceProvider();

            var userRepository = ServiceProvider.GetRequiredService<IRepository<User>>();

            // Add some data
            userRepository.Add(new User
            {
                Id = 1,
                Email = "admin@example.com",
                PasswordHash = "hashed-password",
                CreatedAt = DateTime.UtcNow
            });


            var app = builder.Build();

            // --- Middleware ---------------------------------------------------------
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // --- Endpoints ---------------------------------------------------------
            app.MapUserEndpoints();
            app.MapPhotoEndpoints();

            app.Run();

        }
    }
}