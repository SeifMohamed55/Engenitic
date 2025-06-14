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
        var cloudinaryService = scope.ServiceProvider.GetRequiredService<ICloudinaryService>();
        var uploadingService = scope.ServiceProvider.GetRequiredService<IUploadingService>();
        var hashingService = scope.ServiceProvider.GetRequiredService<IHashingService>();

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
                Email = "instructor@example.com",
                CreatedAt = DateTime.UtcNow,
                FullName = "Abdo Khaled",
            };
            var res3 = await userManager.CreateAsync(user3, "P@ssw0rd");
            await userManager.AddToRoleAsync(user3, Roles.Instructor);

            await context.Database.ExecuteSqlRawAsync(@"
                CREATE OR REPLACE FUNCTION short_description(description TEXT)
                RETURNS TEXT AS $$
                SELECT string_agg(word, ' ') || '...'
                FROM (
                    SELECT unnest(string_to_array(description, ' ')) AS word
                    LIMIT 3
                ) AS words;
                $$ LANGUAGE SQL IMMUTABLE;
                ");
        }

        if (!context.Set<FileHash>().Any())
        {
            using var courseImageStream = await DownloadCloudinaryImageAsStreamAsync
                (cloudinaryService.GetImageUrl(ICloudinaryService.DefaultCourseImagePublicId, "1"));


            using var userImageStream = await DownloadCloudinaryImageAsStreamAsync
                (cloudinaryService.GetImageUrl(ICloudinaryService.DefaultUserImagePublicId, "1"));

            var courseImageHash = await hashingService.HashWithxxHash(courseImageStream);
            var userImageHash  = await hashingService.HashWithxxHash(userImageStream);

            var defaultCourseImage = new FileHash()
            {
                PublicId = ICloudinaryService.DefaultCourseImagePublicId,
                Type = Domain.Enums.CloudinaryType.CourseImage,
                Hash = courseImageHash,
                Version = "1" 
            };

            var defaultUserImage = new FileHash()
            {
                PublicId = ICloudinaryService.DefaultUserImagePublicId,
                Type = Domain.Enums.CloudinaryType.UserImage,
                Hash = userImageHash,
                Version = "1" 
            };

            context.Set<FileHash>().Add(defaultCourseImage);
            context.Set<FileHash>().Add(defaultUserImage);


            var user1 = await userManager.FindByEmailAsync("itproject656@gmail.com");
            if(user1 != null)
                user1.FileHashes.Add(defaultUserImage);

            var user2 = await userManager.FindByEmailAsync("student@example.com");
            if (user2 != null)
                user2.FileHashes.Add(defaultUserImage);

            var user3 = await userManager.FindByEmailAsync("instructor@example.com");
            if (user3 != null)
                user3.FileHashes.Add(defaultUserImage);

            await context.SaveChangesAsync();

        }
  
    }

    private static async Task<Stream> DownloadCloudinaryImageAsStreamAsync(string cloudinaryImageUrl)
    {
        var httpClient = new HttpClient();

        try
        {
            var response = await httpClient.GetAsync(cloudinaryImageUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync(); // You own the stream, dispose it when done
        }
        catch (HttpRequestException ex)
        {
            // Handle network errors, etc.
            throw new Exception("Error downloading image from Cloudinary", ex);
        }
    }


}
