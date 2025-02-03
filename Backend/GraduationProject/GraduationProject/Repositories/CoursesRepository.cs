using GraduationProject.Models.DTOs;
using GraduationProject.Models;
using GraduationProject.Services;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Repositories
{

    public interface ICourseRepository : IRepository<Course>
    {
        Task<CourseDTO?> GetById(int id);
        Task<PaginatedList<CourseDTO>> GetPageOfCourses(int index = 1);
        Task<PaginatedList<CourseDTO>> GetPageOfCoursesWithHidden(int index = 1);
        Task<PaginatedList<CourseDTO>> GetPageOfCoursesBySearching(string searchTerm, int index = 1);
    }
    public class CoursesRepository : Repository<Course>, ICourseRepository
    {
        public CoursesRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<CourseDTO?> GetById(int id)
        {
            var course = await _dbSet.FindAsync(id);
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


    }
}
