using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.Application.Services;
using GraduationProject.Common.Extensions;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Models;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using System.Data;

namespace GraduationProject.Infrastructure.Data.Repositories
{

    public interface ICourseRepository : IRepository<Course>
    {
        Task<CourseDetailsResponse?> GetDetailsById(int id);
        Task<PaginatedList<CourseDTO>> GetPageOfCourses(int index = 1);
        Task<PaginatedList<CourseDTO>> GetPageOfHiddenCourses(int index = 1);
        Task<PaginatedList<CourseDTO>> GetPageOfCoursesBySearching(string searchTerm, int index = 1);
        Task<CourseStatistics?> GetCourseStatistics(int courseId);
        Task<PaginatedList<CourseDTO>> GetInstructorCourses(int instructorId, int index);

        // Edit, Add, Remove
        Task<Course> MakeCourse(RegisterCourseRequest course, FileHash hash);
        Task<Course> EditCourse(EditCourseRequest course);
        Task<PaginatedList<CourseDTO>> GetPageOfCoursesByTag(string tag, int index);
        Task AddCourseToTag(int courseId, List<TagDTO> tag);
        Task<int?> GetCourseInstructorId(int courseId);
        Task<Course?> GetCourseWithImageAndInstructor(int id);
        Task<EditCourseRequest?> GetCourseWithQuizes(int courseId);

        //Task<bool> AddListOfCourses(List<RegisterCourseRequest> courses);
    }
    public class CoursesRepository : Repository<Course>, ICourseRepository
    {

        private readonly DbSet<Tag> _tags;
        private readonly AppDbContext _context;
        public CoursesRepository(AppDbContext context) : base(context)
        {
            _tags = context.Set<Tag>();
            _context = context;
        }

        public async Task<CourseDetailsResponse?> GetDetailsById(int id)
        {
            var course = await _dbSet
                .Include(x => x.FileHash)
                .Include(x => x.Instructor).FirstOrDefaultAsync(x => x.Id == id);
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

        public async Task<Course?> GetCourseWithImageAndInstructor(int id)
        {
            return await _dbSet
                .Include(x => x.FileHash)
                .Include(x => x.Instructor)
                .FirstOrDefaultAsync(x => x.Id == id);
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
            return await _dbSet.Include(x => x.Enrollments)
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

        public Task<PaginatedList<CourseDTO>> GetPageOfCoursesByTag(string tag, int index)
        {
            var query = _dbSet
                            .Where(c => c.Tags.Any(t => t.Value == tag))
                            .OrderBy(x => x.Title)
                            .DTOProjection();

            return PaginatedList<CourseDTO>.CreateAsync(query, index);
        }

        public async Task<Course> MakeCourse(RegisterCourseRequest courseReq, FileHash hash)
        {
            var tags = await _tags
                .Where(t => courseReq.Tags.Select(x => x.Id).Contains(t.Id))
                .ToListAsync();

            Course courseDb = new Course(courseReq, tags, hash);

            Insert(courseDb);

            return courseDb;
        }

        public async Task<Course> EditCourse(EditCourseRequest courseReq)
        {
            var tags = await _tags
                .Where(t => courseReq.Tags.Select(x => x.Id).Contains(t.Id))
                .ToListAsync();

            var dbCourse = await _dbSet
                .Include(c => c.Quizes)
                .Include(c => c.Tags)
                .Include(c => c.FileHash)
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(x => x.Id == courseReq.Id);

            if (dbCourse == null)
                throw new Exception();

            dbCourse.Quizes.Clear();
            dbCourse.Tags.Clear();

            dbCourse.UpdateFromRequest(courseReq, tags);

            return dbCourse;
        }

        public async Task AddCourseToTag(int courseId, List<TagDTO> tags)
        {
            var course = await _dbSet.FirstOrDefaultAsync(x => x.Id == courseId);
            if (course == null)
                throw new ArgumentNullException("course is not found");

            var dbTags = await _tags.Where(t => tags.Select(x => x.Id).Contains(t.Id)).ToListAsync();
            course.Tags.AddRange(dbTags);
            Update(course);

        }

        public async Task<int?> GetCourseInstructorId(int courseId)
        {
            var res = await _dbSet.Select(x=> new { x.InstructorId , x.Id})
                .FirstOrDefaultAsync(x => x.Id == courseId);

            if (res == null)
                return null;

            return res.InstructorId;
        }

        public async Task<EditCourseRequest?> GetCourseWithQuizes(int courseId)
        {
            return await GetCourseWithQuizesCompiled(_context, courseId);
        }



        static readonly Func<AppDbContext, int, Task<EditCourseRequest?>> GetCourseWithQuizesCompiled =
     EF.CompileAsyncQuery((AppDbContext context, int courseId) =>
         context.Courses
             .Include(x => x.Tags)
             .Include(x => x.Quizes)
                 .ThenInclude(x => x.Questions)
                     .ThenInclude(q => q.Answers)
             .AsSingleQuery()
             .Select(x => new EditCourseRequest
             {
                 Id = x.Id,
                 Code = x.Code,
                 Title = x.Title,
                 Description = x.Description,
                 Requirements = x.Requirements,
                 InstructorId = x.InstructorId,
                 Tags = x.Tags.Select(t => new TagDTO
                 {
                     Id = t.Id,
                     Value = t.Value
                 }).ToList(),
                 Quizes = x.Quizes.Select(q => new QuizDTO
                 {
                     Id = q.Id,
                     Title = q.Title,
                     Position = q.Position,
                     Questions = q.Questions.Select(qq => new QuestionDTO
                     {
                         Id = qq.Id,
                         QuestionText = qq.QuestionText,
                         Position = qq.Position,
                         Answers = qq.Answers.Select(a => new AnswerDTO
                         {
                             Id = a.Id,
                             AnswerText = a.AnswerText,
                             IsCorrect = a.IsCorrect,
                             Position = a.Position
                         }).OrderBy(x => x.Position).ToList()
                     }).OrderBy(x => x.Position).ToList()
                 }).OrderBy(x=> x.Position).ToList()
             })
             .FirstOrDefault(x => x.Id == courseId));


        /*        public async Task<bool> AddListOfCourses(List<RegisterCourseRequest> courses)
                {
                    var dbCourses = courses.Select(x => new Course(x, x.Tags.Select(x => new Tag(x)).ToList()));
                    using (var transaction = _context.Database.BeginTransaction())
                    {
                        try
                        {
                            await _context.Courses.AddRangeAsync(dbCourses);
                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();
                            return true;
                        }
                        catch
                        {
                            await transaction.RollbackAsync();
                            return false;
                        }

                    }
                }*/
    }
}
