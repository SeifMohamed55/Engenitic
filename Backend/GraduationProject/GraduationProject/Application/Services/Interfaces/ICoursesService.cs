using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.Domain.DTOs;

namespace GraduationProject.Application.Services.Interfaces
{
    public interface ICoursesService
    {
        Task<PaginatedList<CourseDTO>> GetPageOfCourses(int index);
        Task<PaginatedList<CourseDTO>> SearchOnPageOfCourses(string search, int index);
        Task<PaginatedList<CourseDTO>> GetPageOfCoursesByTag(string tag, int index);
        Task<CourseDetailsResponse?> GetCourseDetailsById(int courseId);
        Task<List<TagDTO>> GetAllTagsAsync();
        Task<PaginatedList<CourseDTO>> GetInstructorCourses(int instructorId, int index);
        Task<CourseStatistics?> GetCourseStatistics(int courseId);
        Task<CourseDTO> AddCourse(RegisterCourseRequest course);
        Task<ServiceResult<CourseDetailsResponse>> EditCourse(EditCourseRequest course);
        Task DeleteCourse(int courseId);
        Task<ServiceResult<bool>> EditCourseImage(IFormFile image, int courseId);
        Task<int?> GetCourseInstructorId(int courseId);
        Task<EditCourseRequest?> GetCourseWithQuizes(int courseId);
        Task<ServiceResult<List<QuizTitleResponse>>> GetQuizesTitles(int courseId);
        Task<List<CourseDTO>> GetRandomCourses(int numberOfCourses);
    }

}
