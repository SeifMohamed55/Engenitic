using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.Domain.DTOs;

namespace GraduationProject.Application.Services.Interfaces
{
    public interface IStudentService
    {
        Task<PaginatedList<EnrollmentDTO>> GetStudentEnrollments(int studentId, int index);
        Task EnrollOnCourse(StudentEnrollmentRequest enrollment);
        Task<ServiceResult<EnrollmentDTO>> GetStudentEnrollment(int studentId, int courseId);
        Task<ServiceResult<StageResponse>> GetEnrollmentStage(int enrollmentId, int stage, int studentId);
        Task<ServiceResult<StageResponse>> GetEnrollmentCurrentStage(int enrollmentId, int studentId);
        Task<ServiceResult<bool>> EnrollmentExists(int studentId, int courseId);
        Task<ServiceResult<UserQuizAttemptDTO>> AttemptQuiz(UserQuizAttemptDTO quizAttempt);
    }

}

