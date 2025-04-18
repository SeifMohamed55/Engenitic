using GraduationProject.API.Requests;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data;

namespace GraduationProject.Application.Services
{

    public interface IReviewService
    {
        Task<ServiceResult<bool>> AddReviewAsync(AddReviewRequestModel review);
        Task<ServiceResult<bool>> UpdateReviewAsync(EditReviewRequestModel review);
        Task<ServiceResult<bool>> DeleteReviewAsync(int reviewId);
        Task<PaginatedList<ReviewDTO>> GetReviewsByCourseIdAsync(int courseId, int index);
    }
    public class ReviewService : IReviewService
    {
        private IUnitOfWork _unitOfWork;
        public ReviewService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private async Task<bool> UpdateCourseAvg(int courseId)
        {
            var courseRatingTask = _unitOfWork.ReviewRepository.GetAverageCourseRatingAsync(courseId);
            var courseTask =  _unitOfWork.CourseRepo.GetByIdAsync(courseId);
            await Task.WhenAll(
               courseRatingTask,
               courseTask
            );
            var course = await courseTask;
            if (course == null)
            {
                return false;
            }
            course.AverageRating = await courseRatingTask;
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<ServiceResult<bool>> AddReviewAsync(AddReviewRequestModel review)
        {
            try
            {
                _unitOfWork.ReviewRepository.AddReview(review);
                await _unitOfWork.SaveChangesAsync();

                var success = await UpdateCourseAvg(review.CourseId);
                if (!success)
                {
                    return ServiceResult<bool>.Failure("Something wrong happened");
                }
                return ServiceResult<bool>.Success(success);
            }
            catch
            {
                return ServiceResult<bool>.Failure("Something wrong happened");
            }

        }

        public async Task<ServiceResult<bool>> DeleteReviewAsync(int reviewId)
        {
            try
            {
                _unitOfWork.ReviewRepository.Delete(reviewId);
                await _unitOfWork.SaveChangesAsync();
                return ServiceResult<bool>.Success(true);
            }
            catch
            {
                return ServiceResult<bool>.Failure("Something wrong happened");
            }
        }

        public async Task<PaginatedList<ReviewDTO>> GetReviewsByCourseIdAsync(int courseId, int index)
        {
           return await _unitOfWork.ReviewRepository.GetReviewsByCourseIdAsync(courseId, index);
        }

        public async Task<ServiceResult<bool>> UpdateReviewAsync(EditReviewRequestModel review)
        {
            try
            {
                _unitOfWork.ReviewRepository.EditReview(review);
                await _unitOfWork.SaveChangesAsync();

                var success = await UpdateCourseAvg(review.CourseId);
                if (!success)
                {
                    return ServiceResult<bool>.Failure("Something wrong happened");
                }
                return ServiceResult<bool>.Success(success);
            }
            catch
            {
                return ServiceResult<bool>.Failure("Something wrong happened");
            }
        }
    }
}

