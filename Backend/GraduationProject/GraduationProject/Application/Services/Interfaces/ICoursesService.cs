using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.Domain.DTOs;

namespace GraduationProject.Application.Services.Interfaces
{
    public interface ICoursesService
    {
        Task<ServiceResult<PaginatedList<CourseDTO>>> GetPageOfCourses(int index);
        Task<ServiceResult<PaginatedList<CourseDTO>>> SearchOnPageOfCourses(string search, int index);
        Task<ServiceResult<PaginatedList<CourseDTO>>> GetPageOfCoursesByTag(string tag, int index);
        Task<ServiceResult<CourseDetailsResponse>> GetCourseDetailsById(int courseId);
        Task<List<TagDTO>> GetAllTagsAsync();
        Task<ServiceResult<PaginatedList<CourseDTO>>> GetInstructorCourses(int instructorId, int index);
        Task<ServiceResult<CourseStatistics>> GetCourseStatistics(int courseId);
        Task<ServiceResult<CourseDTO>> AddCourse(RegisterCourseRequest course);
        Task<ServiceResult<CourseDetailsResponse>> EditCourse(EditCourseRequest course);
        Task<ServiceResult<bool>> DeleteCourse(int courseId);
        Task<ServiceResult<bool>> EditCourseImage(IFormFile image, int courseId);
        Task<int?> GetCourseInstructorId(int courseId);
        Task<ServiceResult<EditCourseRequest>> GetCourseWithQuizes(int courseId);
        Task<ServiceResult<List<QuizTitleResponse>>> GetQuizesTitles(int courseId);
        Task<ServiceResult<List<CourseDTO>>> GetRandomCourses(int numberOfCourses);
    }

}
