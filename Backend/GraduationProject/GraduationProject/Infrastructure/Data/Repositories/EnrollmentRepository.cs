using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.Application.Services;
using GraduationProject.Common.Extensions;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data.Repositories.Base;
using GraduationProject.Infrastructure.Data.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Infrastructure.Data.Repositories
{
    public class EnrollmentRepository : BulkRepository<UserEnrollment, int>, IEnrollmentRepository
    {
        private readonly DbSet<Course> _courses;

        public EnrollmentRepository(AppDbContext context) : base(context)
        {
            _courses = context.Set<Course>();
        }

        public async Task<UserEnrollment> EnrollOnCourse(StudentEnrollmentRequest enrollment)
        {
            var course = await _courses.Select(x => new { x.Stages, x.Id, x.hidden })
                .FirstOrDefaultAsync(x => x.Id == enrollment.CourseId);

            if (course == null)
                throw new ArgumentNullException("Course not found");

            if (course.hidden)
                throw new InvalidOperationException("Course is not available");

            var dbEnrollment = new UserEnrollment
            {
                UserId = enrollment.StudentId,
                CourseId = enrollment.CourseId,
                IsCompleted = false,
                CurrentStage = 0,
                EnrolledAt = DateTime.UtcNow,
                TotalStages = course.Stages
            };
            Insert(dbEnrollment);
            return dbEnrollment;
        }


        public async Task<PaginatedList<EnrollmentDTO>> GetStudentEnrolledCourses(int studentId, int index)
        {
            var query = _dbSet
                .Where(x => x.UserId == studentId && !x.Course.hidden)
                .DTOProjection();
               
            return await PaginatedList<EnrollmentDTO>.CreateAsync(query, index);
        }

        public Task<EnrollmentDTO?> GetStudentEnrollmentDTO(int studentId, int courseId)
        {
            return _dbSet
                .Where(x => x.UserId == studentId && x.CourseId == courseId)
                .DTOProjection()
                .FirstOrDefaultAsync();
        }

        public async Task<UserEnrollment?> GetStudentEnrollment(int studentId, int courseId)
        {
            var enrollment = await _dbSet
                .FirstOrDefaultAsync(x => x.UserId == studentId && x.CourseId == courseId);

            return enrollment;
        }
        public async Task<bool> ExistsAsync(int studentId, int courseId)
        {
            return await _dbSet.AnyAsync(x => x.UserId == studentId && x.CourseId == courseId);
        }

        public async Task<int> GetTotalEnrolledOnCourse(int courseId)
        {
            return await _dbSet.CountAsync(x => x.CourseId == courseId);
        }

    }
}
