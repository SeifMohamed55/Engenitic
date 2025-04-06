using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data;
using System.Net;

namespace GraduationProject.Application.Services
{

    public interface IStudentService
    {
        Task<PaginatedList<EnrollmentDTO>> GetStudentEnrollments(int studentId, int index);
        Task EnrollOnCourse(StudentEnrollmentRequest enrollment);
        Task<ServiceResult<EnrollmentDTO>> GetStudentEnrollment(int studentId, int courseId);
        Task<ServiceResult<QuizDTO>> GetEnrollmentStage(int enrollmentId, int stage, int studentId);
        Task<ServiceResult<QuizDTO>> GetEnrollmentCurrentStage(int enrollmentId, int studentId);
        Task<bool> EnrollmentExists(int studentId, int courseId);
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

        public async Task<bool> EnrollmentExists(int studentId, int courseId)
        {
            return await _unitOfWork.EnrollmentRepo.ExistsAsync(studentId, courseId);
        }

        public async Task<ServiceResult<EnrollmentDTO>> GetStudentEnrollment(int studentId, int courseId)
        {
            var enrollmentDTO = await _unitOfWork.EnrollmentRepo
                   .GetStudentEnrollmentDTO(studentId, courseId);

            if(enrollmentDTO == null)           
                return  ServiceResult<EnrollmentDTO>.Failure("Enrollment not found");
            

            enrollmentDTO.Course.Image.ImageURL = _cloudinaryService
                .GetImageUrl(enrollmentDTO.Course.Image.ImageURL, enrollmentDTO.Course.Image.Version);

            return ServiceResult<EnrollmentDTO>.Success(enrollmentDTO);
        }

        public async Task<ServiceResult<QuizDTO>> GetEnrollmentStage(int enrollmentId, int stage, int studentId)
        {
            var enrollmentResult = await GetAndValidateEnrollment(enrollmentId, studentId);

            if (enrollmentResult.TryGetData(out var enrollment))
            {
                if(stage > enrollment.CurrentStage || stage < 0)
                    return ServiceResult<QuizDTO>.Failure("Invalid Stage");

                return await GetQuizForStage(enrollment.CourseId, stage);
            }
            else
                return ServiceResult<QuizDTO>.Failure(enrollmentResult.Message);

        }

        public async Task<ServiceResult<QuizDTO>> GetEnrollmentCurrentStage(int enrollmentId, int studentId)
        {
            var enrollmentResult = await GetAndValidateEnrollment(enrollmentId, studentId);

            if (enrollmentResult.TryGetData(out var enrollment))
                return await GetQuizForStage(enrollment.CourseId, enrollment.CurrentStage);
            
            else
                return ServiceResult<QuizDTO>.Failure(enrollmentResult.Message);
        }

        // Helper method 
        private async Task<ServiceResult<UserEnrollment>> GetAndValidateEnrollment(int enrollmentId, int studentId)
        {
            var enrollment = await _unitOfWork.EnrollmentRepo.GetByIdAsync(enrollmentId);

            if (enrollment == null)
                return ServiceResult<UserEnrollment>.Failure("Enrollment not found");

            if(enrollment.UserId != studentId)
                return ServiceResult<UserEnrollment>.Failure("You are not authorized to access this enrollment");

            if (enrollment.CurrentStage == 0)
            {
                enrollment.CurrentStage = 1;
                await _unitOfWork.SaveChangesAsync();
            }

            return ServiceResult<UserEnrollment>.Success(enrollment);
        }

        // Helper method
        private async Task<ServiceResult<QuizDTO>> GetQuizForStage(int courseId, int stage)
        {
            var quizDTO = await _unitOfWork.QuizRepo.GetQuizByCourseAndPosition(courseId, stage);

            if (quizDTO == null)
                return ServiceResult<QuizDTO>.Failure("Quiz not found");

            return ServiceResult<QuizDTO>.Success(quizDTO);
        }
    }

}

