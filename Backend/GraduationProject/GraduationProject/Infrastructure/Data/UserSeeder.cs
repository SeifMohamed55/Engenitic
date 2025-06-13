using GraduationProject.Application.Services.Interfaces;
using GraduationProject.API.Requests;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Google;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using GraduationProject.Domain.Models;
using GraduationProject.Common.Constants;

namespace GraduationProject.Infrastructure.Data;
public static class AppDbSeeder
{

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Apply any pending migrations
        await context.Database.MigrateAsync();

        if (!context.Roles.Any())
        {
            context.Roles.AddRange(
                new Role()
                {
                    Name = Roles.Admin
                },
                new Role()
                {
                    Name = Roles.Student
                },
                new Role()
                {
                    Name = Roles.Instructor
                },
                new Role()
                {
                    Name = Roles.UnverifiedInstructor
                }
            );
            await context.SaveChangesAsync();
        }

        var _authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();

        if (!context.Users.Any())
        {
            // Seed Admin
            await _authService.Register(new RegisterCustomRequest
            {
                Username = "admin",
                Email = "itproject656@gmail.com",
                Password = "P@ssw0rd",
                ConfirmPassword = "P@ssw0rd",
                Role = "admin"
            }, false);

            // Seed Student
            await _authService.Register(new RegisterCustomRequest
            {
                Username = "student",
                Email = "student@example.com",
                Password = "P@ssw0rd",
                ConfirmPassword = "P@ssw0rd",
                Role = "student"
            }, false);

            // Seed Instructor
            await _authService.Register(new RegisterCustomRequest
            {
                Username = "instructor",
                Email = "instructor@example.com",
                Password = "P@ssw0rd",
                ConfirmPassword = "P@ssw0rd",
                Role = "instructor"
            }, false);
        }
    }
}
