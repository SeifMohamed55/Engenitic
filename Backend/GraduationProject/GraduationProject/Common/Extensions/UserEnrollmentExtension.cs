using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Common.Extensions
{
    public static class UserEnrollmentExtension
    {
        public static IQueryable<EnrollmentDTO> DTOProjection(this IQueryable<UserEnrollment> query)
        {
            return query
                .Include(x => x.Course)
                .ThenInclude(x => x.Instructor)
                .Include(x => x.Course.FileHash)
                .OrderBy(x => x.IsCompleted)
                .ThenBy(x => x.Course.Title)
                .Select(enrollment => new EnrollmentDTO()
                {
                    Id = enrollment.Id,
                    EnrolledAt = enrollment.EnrolledAt,
                    CurrentStage = enrollment.CurrentStage,
                    IsCompleted = enrollment.IsCompleted,
                    TotalStages = enrollment.TotalStages,
                    Progress = enrollment.IsCompleted ?
                                    100.0f : 
                                    Math.Clamp((float)(enrollment.CurrentStage - 1) / enrollment.TotalStages * 100.0f, 0, 100),
                    CourseId = enrollment.CourseId,
                    Course = new CourseDTO()
                    {
                        Id = enrollment.Course.Id,
                        Title = enrollment.Course.Title,
                        Code = enrollment.Course.Code,
                        Stages = enrollment.Course.Stages,
                        Description = MyDbFunctions.ShortDescription(enrollment.Course.Description),
                        InstructorName = enrollment.Course.Instructor.FullName,
                        Requirements = enrollment.Course.Requirements,
                        Image = new()
                        {
                            ImageUrl = enrollment.Course.FileHash.PublicId,
                            Name = "Course Image",
                            Hash = enrollment.Course.FileHash.Hash,
                            Version = enrollment.Course.FileHash.Version,
                        }
                    },
                });
        }
    }
}
