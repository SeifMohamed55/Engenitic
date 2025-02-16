using GraduationProject.Models.DTOs;
using GraduationProject.Models;
using GraduationProject.Services;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1;
using System.Diagnostics;
using GraduationProject.Controllers.ApiRequest;
using Ganss.Xss;

namespace GraduationProject.Repositories
{

    public interface ICourseRepository : IRepository<Course>
    {
        Task<CourseDTO?> GetById(int id);
        Task<PaginatedList<CourseDTO>> GetPageOfCourses(int index = 1);
        Task<PaginatedList<CourseDTO>> GetPageOfCoursesWithHidden(int index = 1);
        Task<PaginatedList<CourseDTO>> GetPageOfCoursesBySearching(string searchTerm, int index = 1);
        Task<CourseStatistics?> GetCourseStatistics(int courseId);
        Task<PaginatedList<EnrollmentDTO>> GetStudentEnrolledCourses(int studentId, int index);
        Task<PaginatedList<CourseDTO>> GetInstructorCourses(int instructorId, int index);

        // Edit, Add, Remove
        Task<Course> AddCourse(RegisterCourseRequest course);
        Task<Course> EditCourse(CourseDTO course);
        Task<bool> DisableCourse(int courseId);

    }
    public class CoursesRepository : Repository<Course>, ICourseRepository
    {

        private readonly DbSet<UserEnrollment> _enrollments;
        public CoursesRepository(AppDbContext context) : base(context)
        {
            _enrollments = context.Set<UserEnrollment>();
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


        public async Task<CourseStatistics?> GetCourseStatistics(int courseId)
        {
            return await _dbSet.Include(x=> x.Enrollments)
                .Select(x => new CourseStatistics()
                {
                    CourseId = x.Id,
                    UserEmails = x.Enrollments.Select(e => e.User.Email),
                    TotalEnrollments = x.Enrollments.Count,
                    TotalCompleted = x.Enrollments.Count(e => e.IsCompleted == true),
                })
                .FirstOrDefaultAsync(x => x.CourseId == courseId)
;
        }

        public async Task<PaginatedList<CourseDTO>> GetInstructorCourses(int instructorId, int index)
        {
            var query =  _dbSet
                .Where(x => x.InstructorId == instructorId)
                .Include(x=> x.Instructor)
                .OrderBy(x => x.Title)
                .Select(x=> new CourseDTO(x));

            return await PaginatedList<CourseDTO>.CreateAsync(query, index);
           
        }

        public async Task<PaginatedList<EnrollmentDTO>> GetStudentEnrolledCourses(int studentId, int index)
        {
            var query = _enrollments.Include(x=> x.Course)
                .Where(x => x.UserId == studentId)
                .OrderBy(x => x.IsCompleted)
                .ThenBy(x=> x.Course.Title)
                .Select(x => new EnrollmentDTO(x));

            return await PaginatedList<EnrollmentDTO>.CreateAsync(query, index);

        }

        public async Task<Course> AddCourse(RegisterCourseRequest courseReq)
        {
            Course courseDb = new Course(courseReq);

            await this.AddAsync(courseDb);

            if (ImageHelper.IsValidImageType(courseReq.Image))
            {
                Debug.Assert(courseReq.Image != null);

                var extension = Path.GetExtension(courseReq.Image.FileName).ToLower();
                extension = (extension == ".jpeg" || extension == ".jpg") ?
                                extension : ImageHelper.GetImageExtenstion(courseReq.Image.ContentType);

                var imageURL = "course_" + courseDb.Id + "." + extension;

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(),
                                        "uploads", "images", "courses");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, imageURL);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await courseReq.Image.CopyToAsync(stream);
                }
                courseDb.ImageUrl = imageURL;
                await this.UpdateAsync(courseDb);
            }
            return courseDb;
        }

        public Task<Course> EditCourse(CourseDTO course)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DisableCourse(int courseId)
        {
            throw new NotImplementedException();
        }
    }
}
