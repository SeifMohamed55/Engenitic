using GraduationProject.Models.DTOs;
using GraduationProject.Models;
using GraduationProject.Services;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1;

namespace GraduationProject.Repositories
{

    public interface ICourseRepository : IRepository<Course>
    {
        Task<CourseDTO?> GetById(int id);
        Task<PaginatedList<CourseDTO>> GetPageOfCourses(int index = 1);
        Task<PaginatedList<CourseDTO>> GetPageOfCoursesWithHidden(int index = 1);
        Task<PaginatedList<CourseDTO>> GetPageOfCoursesBySearching(string searchTerm, int index = 1);
        Task<List<CourseDTO>> GetInstructorCourses(int id);
        Task<List<CourseStatistics>> GetCourseStatistics(int courseId);

    }
    public class CoursesRepository : Repository<Course>, ICourseRepository
    {

        public CoursesRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<CourseDTO?> GetById(int id)
        {
            var course = await _dbSet.Include(x => x.Instructor).FirstOrDefaultAsync(x=> x.Id == id);
            if (course == null)
                return null;

            return new CourseDTO(course);
        }

        public async Task<PaginatedList<CourseDTO>> GetPageOfCourses(int index = 1)
        {
            var courses = _dbSet
                .Include(x => x.Instructor)
                .Where(x => x.hidden == false)
                .OrderBy(x => x.Title)
                .Select(x => new CourseDTO(x));

            return await PaginatedList<CourseDTO>.CreateAsync(courses, index);
        }

        public async Task<PaginatedList<CourseDTO>> GetPageOfCoursesWithHidden(int index = 1)
        {
            var courses = _dbSet
                .Include(x => x.Instructor)
                .OrderBy(x => x.Title)
                .Select(x => new CourseDTO(x));

            return await PaginatedList<CourseDTO>.CreateAsync(courses, index);
        }

        public async Task<PaginatedList<CourseDTO>> GetPageOfCoursesBySearching(string searchTerm, int index = 1)
        {
            var courses = _dbSet
                .Include(x => x.Instructor)
                .Where(x => x.hidden == false)
                .Where(x => x.Title.Contains(searchTerm) || 
                            x.Description.Contains(searchTerm))
                .OrderBy(x => x.Title)
                .Select(x => new CourseDTO(x));
            return await PaginatedList<CourseDTO>.CreateAsync(courses, index);
        }

        public async Task<List<CourseDTO>> GetInstructorCourses(int instructorId)
        {
            return await _dbSet
                .Where(x => x.InstructorId == instructorId)
                .Select(x=> new CourseDTO(x))
                .ToListAsync();
        }

        public async Task<List<CourseStatistics>> GetCourseStatistics(int courseId)
        {
            return await _dbSet.Include(x=> x.Enrollments)
                .Where(x => x.Id == courseId)
                .Select(x => new CourseStatistics()
                {
                    UserEmails = x.Enrollments.Select(e => e.User.Email),
                    TotalEnrollments = x.Enrollments.Count,
                    TotalCompleted = x.Enrollments.Count(e => e.IsCompleted == true),
                })
                .ToListAsync();
        }
    }
}
