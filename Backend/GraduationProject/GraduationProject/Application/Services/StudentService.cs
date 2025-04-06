using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.Domain.DTOs;
using GraduationProject.Infrastructure.Data;
using System.Net;

namespace GraduationProject.Application.Services
{

    public interface IStudentService
    {
        Task<PaginatedList<EnrollmentDTO>> GetStudentEnrollments(int studentId, int index);
        Task EnrollOnCourse(StudentEnrollmentRequest enrollment);
        Task<ServiceResult<EnrollmentDTO>> GetStudentEnrollment(int studentId, int courseId);
    }

    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinaryService;

        public StudentService(IUnitOfWork unitOfWork, ICloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<PaginatedList<EnrollmentDTO>> GetStudentEnrollments(int studentId, int index)
        {
            var enrollments = await _unitOfWork.EnrollmentRepo.GetStudentEnrolledCourses(studentId, index);
            enrollments.ForEach(x =>
            {
                x.Course.Image.ImageURL = _cloudinaryService
                 .GetImageUrl(x.Course.Image.ImageURL, x.Course.Image.Version);
            });
            return enrollments;
        }

        public async Task EnrollOnCourse(StudentEnrollmentRequest enrollment)
        {     
            await _unitOfWork.EnrollmentRepo.EnrollOnCourse(enrollment);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ServiceResult<EnrollmentDTO>> GetStudentEnrollment(int studentId, int courseId)
        {
            var enrollmentDTO = await _unitOfWork.EnrollmentRepo
                   .GetStudentEnrollment(studentId, courseId);

            if(enrollmentDTO == null)           
                return  ServiceResult<EnrollmentDTO>.Failure("Enrollment not found");
            

            enrollmentDTO.Course.Image.ImageURL = _cloudinaryService
                .GetImageUrl(enrollmentDTO.Course.Image.ImageURL, enrollmentDTO.Course.Image.Version);

            return ServiceResult<EnrollmentDTO>.Success(enrollmentDTO);
        }
    }
    
}

