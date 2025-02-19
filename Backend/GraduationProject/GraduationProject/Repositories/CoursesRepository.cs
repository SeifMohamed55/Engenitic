using GraduationProject.Models.DTOs;
using GraduationProject.Models;
using GraduationProject.Services;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1;
using System.Diagnostics;
using GraduationProject.Controllers.ApiRequest;
using Ganss.Xss;
using GraduationProject.Controllers.APIResponses;
using NuGet.Packaging;
using GraduationProject.Data;

namespace GraduationProject.Repositories
{

    public interface ICourseRepository : IRepository<Course>
    {
        Task<CourseDetailsResponse?> GetById(int id);
        Task<PaginatedList<CourseDTO>> GetPageOfCourses(int index = 1);
        Task<PaginatedList<CourseDTO>> GetPageOfHiddenCourses(int index = 1);
        Task<PaginatedList<CourseDTO>> GetPageOfCoursesBySearching(string searchTerm, int index = 1);
        Task<CourseStatistics?> GetCourseStatistics(int courseId);
        Task<PaginatedList<CourseDTO>> GetInstructorCourses(int instructorId, int index);
        Task<string?> GetImageUrl(int courseId); 

        // Edit, Add, Remove
        Task<CourseDTO> AddCourse(RegisterCourseRequest course);
        Task<Course> EditCourse(CourseDTO course);
        Task<bool> DisableCourse(int courseId);
        Task<PaginatedList<CourseDTO>> GetPageOfCoursesByTag(string tag, int index);
        Task<bool> AddCourseToTag(int courseId, List<TagDTO> tag);
    }
    public class CoursesRepository : Repository<Course>, ICourseRepository
    {

        private readonly DbSet<Tag> _tags;
        public CoursesRepository(AppDbContext context) : base(context)
        {
            _tags = context.Set<Tag>();
        }

        public async Task<CourseDetailsResponse?> GetById(int id)
        {
            var course = await _dbSet.Include(x => x.Instructor).FirstOrDefaultAsync(x=> x.Id == id);
            if (course == null)
                return null;

            return new CourseDetailsResponse(course);
        }


        private IQueryable<Course> GetCoursesQuery()
        {
            return _dbSet
                .Where(x => x.hidden == false)
                .Include(x => x.Instructor)
                .OrderBy(x => x.Title);
                
        }

        public async Task<PaginatedList<CourseDTO>> GetPageOfCourses(int index = 1)
        {
            var courses = GetCoursesQuery().DTOProjection();


            return await PaginatedList<CourseDTO>.CreateAsync(courses, index);
        }

        public async Task<PaginatedList<CourseDTO>> GetPageOfHiddenCourses(int index = 1)
        {
            var courses = _dbSet
                .Where(x => x.hidden == true)
                .Include(x => x.Instructor)
                .OrderBy(x => x.Title)
                .DTOProjection();


            return await PaginatedList<CourseDTO>.CreateAsync(courses, index);
        }

        public async Task<PaginatedList<CourseDTO>> GetPageOfCoursesBySearching(string searchTerm, int index = 1)
        {
            var courses = GetCoursesQuery()
                .Where(x => x.Title.Contains(searchTerm) || x.Description.Contains(searchTerm))
                .DTOProjection();

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
            var query = GetCoursesQuery()
                .Where(x => x.InstructorId == instructorId)
                .DTOProjection();
            return await PaginatedList<CourseDTO>.CreateAsync(query, index);
           
        }

        public async Task<CourseDTO> AddCourse(RegisterCourseRequest courseReq)
        {
            var tags = await _tags
                .Where(t => courseReq.Tags.Select(x=> x.Id).Contains(t.Id))
                .ToListAsync();

            Course courseDb = new Course(courseReq, tags);
            courseDb.ImageUrl = "default.jpeg";

            await this.AddAsync(courseDb);

            if (ImageHelper.IsValidImageType(courseReq.Image))
            {
                Debug.Assert(courseReq.Image != null);

                var extension = Path.GetExtension(courseReq.Image.FileName).ToLower();
                extension = (extension == ".jpeg" || extension == ".jpg") ?
                                extension : ImageHelper.GetImageExtenstion(courseReq.Image.ContentType);

                var imageURL = "course_" + courseDb.Id + extension;

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
            return new CourseDTO(courseDb);
        }

        public Task<Course> EditCourse(CourseDTO course)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DisableCourse(int courseId)
        {
            throw new NotImplementedException();
        }

        public async Task<string?> GetImageUrl(int courseId)
        {
           return (await _dbSet.Select(x=> new { x.ImageUrl , x.Id})
                .FirstOrDefaultAsync(x => x.Id == courseId))?.ImageUrl;
        }

        public Task<PaginatedList<CourseDTO>> GetPageOfCoursesByTag(string tag, int index)
        {
            var query = _dbSet
                            .Where(c => c.Tags.Any(t=> t.Value == tag))
                            .OrderBy(x => x.Title)
                            .DTOProjection();

            return  PaginatedList<CourseDTO>.CreateAsync(query, index);
        }

        public async Task<bool> AddCourseToTag(int courseId, List<TagDTO> tags )
        {
            var course = await _dbSet.FirstOrDefaultAsync(x=> x.Id == courseId);
            if (course == null)
                return false;

            try
            {
                var dbTags = await _tags.Where(t => tags.Select(x => x.Id).Contains(t.Id)).ToListAsync();
                course.Tags.AddRange(dbTags);
                await UpdateAsync(course);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
