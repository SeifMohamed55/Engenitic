using GraduationProject.API.Requests;
using GraduationProject.Application.Services;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data.Repositories.Base.Interfaces;

namespace GraduationProject.Infrastructure.Data.Repositories.interfaces
{
    public interface IEnrollmentRepository : IBulkRepository<UserEnrollment, int>, ICustomRepository
    {
        Task<UserEnrollment> EnrollOnCourse(StudentEnrollmentRequest enrollment);
        Task<PaginatedList<EnrollmentDTO>> GetStudentEnrolledCourses(int studentId, int index);
        Task<EnrollmentDTO?> GetStudentEnrollmentDTO(int studentId, int courseId);
        Task<UserEnrollment?> GetStudentEnrollment(int studentId, int courseId);
        Task<bool> ExistsAsync(int studentId, int courseId);
        Task<int> GetTotalEnrolledOnCourse(int courseId);

    }
}
