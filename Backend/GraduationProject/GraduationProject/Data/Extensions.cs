using GraduationProject.Models;
using GraduationProject.Models.DTOs;

namespace GraduationProject.Data
{
    public static class Extensions
    {
        public static IQueryable<CourseDTO> DTOProjection(this IQueryable<Course> query)
        {
            return query.Select(x => new CourseDTO()
            {
                Code = x.Code,
                Description = MyDbFunctions.ShortDescription(x.Description),
                Id = x.Id,
                InstructorName = x.Instructor.FullName,
                Requirements = x.Requirements,
                Stages = x.Stages,
                Title = x.Title,
                Image = new() { ImageURL = $"https://localhost/api/courses/image?id={x.Id}", Name = x.ImageUrl }
            });
        }

        public static IQueryable<AppUserDTO> DTOProjection(this IQueryable<AppUser> query)
        {
            return query.Select(x => new AppUserDTO
            {
                Id = x.Id,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                PhoneRegionCode = x.PhoneRegionCode,
                Image = new() { Name = x.ImageSrc, ImageURL = $"https://localhost/api/users/image?id={x.Id}" },
                UserName = x.FullName,
                Banned = x.Banned
            });
        }

    }
}
