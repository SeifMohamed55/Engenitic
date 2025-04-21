using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.Application.Services;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data.Repositories.Base.Interfaces;

namespace GraduationProject.Infrastructure.Data.Repositories.interfaces
{
    public interface ICourseRepository : IBulkRepository<Course, int>, ICustomRepository
    {
        Task<CourseDetailsResponse?> GetDetailsById(int id);
        Task<PaginatedList<CourseDTO>> GetPageOfCourses(int index = 1);
        Task<PaginatedList<CourseDTO>> GetPageOfHiddenCourses(int index = 1);
        Task<PaginatedList<CourseDTO>> GetPageOfCoursesBySearching(string searchTerm, int index = 1);
        Task<CourseStatistics?> GetCourseStatistics(int courseId);
        Task<PaginatedList<CourseDTO>> GetInstructorCourses(int instructorId, int index);

        // Edit, Add, Remove
        Task<Course> MakeCourse(RegisterCourseRequest course, FileHash hash);
        Task<PaginatedList<CourseDTO>> GetPageOfCoursesByTag(string tag, int index);
        Task AddCourseToTag(int courseId, List<TagDTO> tag);
        Task<int?> GetCourseInstructorId(int courseId);
        Task<Course?> GetCourseWithImageAndInstructor(int id);
        Task<EditCourseRequest?> GetEditCourseRequestWithQuizes(int courseId);
        Task<Course?> GetCourseWithQuizes(int courseId);

        //Task<bool> AddListOfCourses(List<RegisterCourseRequest> courses);

        Task<List<CourseDTO>> GetRandomCourses(int numberOfCourses);
        Task<QuizQuestionAnswerIds?> GetQuizesQuestionAndAnswerIds(int courseId);
    }
}
