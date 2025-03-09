using GraduationProject.Models;
using GraduationProject.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Security.Policy;

namespace GraduationProject.Data
{
    public static class Extensions
    {
        public static IQueryable<CourseDTO> DTOProjection(this IQueryable<Course> query)
        {
            return query
                .Include(x => x.FileHash)
                .Include(x => x.Instructor)
                .Select(x => new CourseDTO()
            {
                Code = x.Code,
                Description = MyDbFunctions.ShortDescription(x.Description),
                Id = x.Id,
                InstructorName = x.Instructor.FullName,
                Requirements = x.Requirements,
                Stages = x.Stages,
                Title = x.Title,
                Image = new() { ImageURL = x.FileHash.PublicId, Name = "", Hash = x.FileHash.Hash }
            });
        }

        public static IQueryable<AppUserDTO> DTOProjection(this IQueryable<AppUser> query)
        {
            return query
                .Include(x => x.FileHashes)
                .Select(x => new AppUserDTO
                {
                    Id = x.Id,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    PhoneRegionCode = x.PhoneRegionCode,
                    UserName = x.FullName,
                    Banned = x.Banned,
                    IsExternal = x.IsExternal,
                    Image = new()
                    {
                        ImageURL = x.FileHashes
                              .Where(z => z.Type == Services.CloudinaryType.UserImage) 
                              .Select(z => z.PublicId)
                              .FirstOrDefault() ?? "",
                        Name = "" ,
                        Hash = x.FileHashes
                              .Where(z => z.Type == Services.CloudinaryType.UserImage)
                              .Select(z => z.Hash)
                              .FirstOrDefault()
                    }
                });
            
        }

    }
}
