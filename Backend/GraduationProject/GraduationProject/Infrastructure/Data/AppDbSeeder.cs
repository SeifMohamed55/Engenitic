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
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        if (!context.Roles.Any())
        {
            await roleManager.CreateAsync(new Role()
            {
                Name = Roles.Admin
            });
            await roleManager.CreateAsync(new Role()
            {
                Name = Roles.Student
            });
            await roleManager.CreateAsync(new Role()
            {
                Name = Roles.Instructor
            });
            await roleManager.CreateAsync(new Role()
            {
                Name = Roles.UnverifiedInstructor
            });

            await context.SaveChangesAsync();
        }

        if (!context.Users.Any())
        {
            // Seed Admin
            var user1 = new AppUser
            {
                UserName = "itproject656@gmail.com",
                Email = "itproject656@gmail.com",
                CreatedAt = DateTime.UtcNow,
                FullName = "Dr.Ahmed Hesham",
            };
            var res1 = await userManager.CreateAsync(user1, "P@ssw0rd");

            await userManager.AddToRoleAsync(user1, Roles.Admin);

            // Seed Student
            var user2 = new AppUser
            {
                UserName = "student@example.com",
                Email = "student@example.com",
                CreatedAt = DateTime.UtcNow,
                FullName = "Seif-Elden Mohamed",
            };
            var res2 = await userManager.CreateAsync(user2, "P@ssw0rd");
            await userManager.AddToRoleAsync(user2, Roles.Student);

            // Seed Instructor

            var user3 = new AppUser
            {
                UserName = "instructor@example.com",
                Email = "P@ssw0rd",
                CreatedAt = DateTime.UtcNow,
                FullName = "Abdo Khaled",
            };
            var res3 = await userManager.CreateAsync(user3, "P@ssw0rd");
            await userManager.AddToRoleAsync(user3, Roles.Instructor);

        }
    }
}
