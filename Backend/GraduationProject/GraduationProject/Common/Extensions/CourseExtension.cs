using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Common.Extensions
{
    public static class CourseExtension
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
                    Image = new() { ImageURL = x.FileHash.PublicId, Name = "CourseImg", Hash = x.FileHash.Hash, Version = x.FileHash.Version }
                });
        }
    }
}
