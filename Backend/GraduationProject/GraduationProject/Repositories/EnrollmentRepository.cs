using GraduationProject.Controllers.ApiRequest;
using GraduationProject.Models;
using GraduationProject.Models.DTOs;
using GraduationProject.Services;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Repositories
{

    public interface IEnrollmentRepository : IRepository<UserEnrollment>
    {
        Task<bool> EnrollOnCourse(StudentEnrollmentRequest enrollment);
        Task<PaginatedList<EnrollmentDTO>> GetStudentEnrolledCourses(int studentId, int index);
    }
    public class EnrollmentRepository : Repository<UserEnrollment>, IEnrollmentRepository
    {
        private readonly DbSet<Course> _courses;

        public EnrollmentRepository(AppDbContext context) : base(context)
        {
            _courses = context.Set<Course>();
        }

        public async Task<bool> EnrollOnCourse(StudentEnrollmentRequest enrollment)
        {
            try
            {
                var totalStages = (await _courses.Select(x => new { x.Stages, x.Id })
                    .FirstOrDefaultAsync(x => x.Id == enrollment.CourseId))?.Stages;

                if (totalStages == null)
                    return false;

                var dbEnrollment = new UserEnrollment
                {
                    UserId = enrollment.StudentId,
                    CourseId = enrollment.CourseId,
                    IsCompleted = false,
                    CurrentStage = 0,
                    EnrolledAt = DateTime.UtcNow,
                    TotalStages = totalStages.Value
                };
                await AddAsync(dbEnrollment);

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public async Task<PaginatedList<EnrollmentDTO>> GetStudentEnrolledCourses(int studentId, int index)
        {
            var query = _dbSet.Include(x => x.Course)
                .ThenInclude(x=> x.Instructor)
                .Where(x => x.UserId == studentId)
                .OrderBy(x => x.IsCompleted)
                .ThenBy(x => x.Course.Title)
                .Select(enrollment => new EnrollmentDTO()
                {
                    Id = enrollment.Id,
                    EnrolledAt = enrollment.EnrolledAt,
                    CurrentStage = enrollment.CurrentStage,
                    IsCompleted = enrollment.IsCompleted,
                    TotalStages = enrollment.TotalStages,
                    Progress = ((float)enrollment.CurrentStage / enrollment.TotalStages) * 100,
                    CourseId = enrollment.CourseId,
                    Course = new CourseDTO()
                    {
                        Id = enrollment.Course.Id,
                        Title = enrollment.Course.Title,
                        Code = enrollment.Course.Code,
                        Stages = enrollment.Course.Stages,
                        Description = GraduationProject.Data.MyDbFunctions.ShortDescription(enrollment.Course.Description),
                        InstructorName = enrollment.Course.Instructor.FullName,
                        Requirements = enrollment.Course.Requirements,
                        Image = new() { ImageURL = $"https://localhost/api/courses/image?id={enrollment.Course.Id}", Name = enrollment.Course.ImageUrl }
                    },
                });
            return await PaginatedList<EnrollmentDTO>.CreateAsync(query, index);

        }
    }
}
