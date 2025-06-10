using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.Application.Services.Interfaces;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data.Interfaces;
using System.Net;

namespace GraduationProject.Application.Services
{

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
                x.Course.Image.FileURL = _cloudinaryService
                 .GetImageUrl(x.Course.Image.FileURL, x.Course.Image.Version);
            });
            return enrollments;
        }

        public async Task EnrollOnCourse(StudentEnrollmentRequest enrollment)
        {
            await _unitOfWork.EnrollmentRepo.EnrollOnCourse(enrollment);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ServiceResult<bool>> EnrollmentExists(int studentId, int courseId)
        {
            try
            {
                var enrollment = await _unitOfWork.EnrollmentRepo.ExistsAsync(studentId, courseId);
                return ServiceResult<bool>.Success(enrollment, "Enrollment exists");
            }
            catch 
            {
                return ServiceResult<bool>.Failure("Something Wrong happened");
            }

        }

        public async Task<ServiceResult<EnrollmentDTO>> GetStudentEnrollment(int studentId, int courseId)
        {
            var enrollmentDTO = await _unitOfWork.EnrollmentRepo
                   .GetStudentEnrollmentDTO(studentId, courseId);

            if(enrollmentDTO == null)           
                return  ServiceResult<EnrollmentDTO>.Failure("Enrollment not found");
            

            enrollmentDTO.Course.Image.FileURL = _cloudinaryService
                .GetImageUrl(enrollmentDTO.Course.Image.FileURL, enrollmentDTO.Course.Image.Version);

            return ServiceResult<EnrollmentDTO>.Success(enrollmentDTO, "Enrollment is retrieved successfully.");
        }

        public async Task<ServiceResult<StageResponse>> GetEnrollmentStage(int enrollmentId, int stage, int studentId)
        {
            var enrollmentResult = await GetAndValidateEnrollment(enrollmentId, studentId);

            if (enrollmentResult.TryGetData(out var enrollment))
            {
                if(stage > enrollment.CurrentStage || stage <= 0)
                    return ServiceResult<StageResponse>.Failure($"You must finish Stage {enrollment.CurrentStage} first.");

                float progress = GetProgress(enrollment);
                var quiz = await GetQuizForStage(enrollment.CourseId, stage);

                if (quiz.TryGetData(out var quizData))
                    return ServiceResult<StageResponse>.Success(new StageResponse(quizData, enrollment.CurrentStage, progress),
                        "Stage retrieved successfully");
                else
                    return ServiceResult<StageResponse>.Failure(quiz.Message);
            }
            else
                return ServiceResult<StageResponse>.Failure(enrollmentResult.Message);

        }

        public async Task<ServiceResult<StageResponse>> GetEnrollmentCurrentStage(int enrollmentId, int studentId)
        {
            var enrollmentResult = await GetAndValidateEnrollment(enrollmentId, studentId);

            if (enrollmentResult.TryGetData(out var enrollment))
            {
                float progress = GetProgress(enrollment);
                var quiz =  await GetQuizForStage(enrollment.CourseId, enrollment.CurrentStage);

                if (quiz.TryGetData(out var quizData))
                    return ServiceResult<StageResponse>.Success(new StageResponse(quizData, enrollment.CurrentStage, progress),
                        "Stage retrieved Successfully");
                else
                    return ServiceResult<StageResponse>.Failure(quiz.Message);
            }
            
            else
                return ServiceResult<StageResponse>.Failure(enrollmentResult.Message);
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
            return ServiceResult<UserEnrollment>.Success(enrollment, "Enrollment retrieved Successfully");
        }

        // Helper method
        private async Task<ServiceResult<QuizDTO>> GetQuizForStage(int courseId, int stage)
        {
            var quizDTO = await _unitOfWork.QuizRepo.GetQuizByCourseAndPosition(courseId, stage);

            if (quizDTO == null)
                return ServiceResult<QuizDTO>.Failure("Quiz not found");

            return ServiceResult<QuizDTO>.Success(quizDTO, "Quiz retrieved Successfully");
        }

        private float GetProgress(UserEnrollment enrollment)
        {
            return enrollment.IsCompleted ?
                100.0f :
                Math.Clamp((float)(enrollment.CurrentStage - 1) / enrollment.TotalStages * 100.0f, 0, 100);
        }

        public async Task<ServiceResult<UserQuizAttemptDTO>> AttemptQuiz(UserQuizAttemptDTO quizAttempt)
        {
            var enrollment = await GetAndValidateEnrollment(quizAttempt.EnrollmentId, quizAttempt.UserId);
            if (enrollment.TryGetData(out var userEnrollment))
            {
                var quizDict = await _unitOfWork.QuizQuestionRepo.GetQuizWithQuestionsByIdAsync(quizAttempt.QuizId);
                int passCount = 0;

                var userQuestionIds = quizAttempt.UserAnswers.Select(x => x.QuestionId).OrderBy(x => x).ToList();
                var databases = quizDict.Values.Select(x=> x.QuestionId).OrderBy(x => x).ToList();

                if(!userQuestionIds.SequenceEqual(databases))
                    return ServiceResult<UserQuizAttemptDTO>.Failure("Invalid Quiz Attempt");


                foreach (var userAnswer in quizAttempt.UserAnswers)
                {
                    if(quizDict.TryGetValue(userAnswer.QuestionId, out var dbQuestion))
                    {
                        if(dbQuestion.AnswerId == userAnswer.AnswerId)
                        {
                            userAnswer.IsCorrect = true;
                            passCount++;
                        }
                    }
                }
                await _unitOfWork.QuizRepo.AddUserQuizAttempt(quizAttempt);
                await _unitOfWork.SaveChangesAsync();

                quizAttempt.Score = passCount.ToString() + "/" + quizDict.Count.ToString();

                if(passCount == quizDict.Count)
                {
                    quizAttempt.IsPassed = true;
                    if (userEnrollment.CurrentStage == userEnrollment.TotalStages)
                        userEnrollment.IsCompleted = true;
                    else
                        userEnrollment.CurrentStage++;
                    await _unitOfWork.SaveChangesAsync();
                }
                return ServiceResult<UserQuizAttemptDTO>.Success(quizAttempt, "Quiz Attempted Succssfully");
            }
            else
            {
                return ServiceResult<UserQuizAttemptDTO>.Failure(enrollment.Message);
            }
        }
    }

}

