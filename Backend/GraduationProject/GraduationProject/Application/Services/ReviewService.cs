using GraduationProject.API.Requests;
using GraduationProject.Application.Services.Interfaces;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data.Interfaces;

namespace GraduationProject.Application.Services
{
    public class ReviewService : IReviewService
    {
        private IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinaryService;
        public ReviewService(IUnitOfWork unitOfWork, ICloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;

        }

        private async Task<bool> UpdateCourseAvg(int courseId)
        {
            var courseRating = await _unitOfWork.ReviewRepository.GetAverageCourseRatingAsync(courseId);
            var course =  await _unitOfWork.CourseRepo.GetByIdAsync(courseId);

            if (course == null)
            {
                return false;
            }
            course.AverageRating = courseRating;
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<ServiceResult<ReviewDTO>> AddReviewAsync(int userId, AddReviewRequestModel review)
        {
            try
            {
                if (await _unitOfWork.ReviewRepository.ReviewExist(userId, review.CourseId))
                {
                    return ServiceResult<ReviewDTO>.Failure("You have already reviewed this course.");
                }

                _unitOfWork.ReviewRepository.AddReview(userId, review);
                await _unitOfWork.SaveChangesAsync();

                var success = await UpdateCourseAvg(review.CourseId);
                var reviewDtoRes = await _unitOfWork.ReviewRepository.GetStudentCourseReview(review.CourseId, userId);

                if (success && reviewDtoRes.TryGetData(out var reviewDTO))
                {
                    return ServiceResult<ReviewDTO>.Success(reviewDTO, "Review Added successfully");
                }

                return ServiceResult<ReviewDTO>.Failure("Something wrong happened");
            }
            catch
            {
                return ServiceResult<ReviewDTO>.Failure("Something wrong happened");
            }

        }


        public async Task<ServiceResult<bool>> DeleteReviewAsync(int userId, int reviewId)
        {
            try
            {
                var dbReview = await _unitOfWork.ReviewRepository.GetByIdAsync(reviewId);
                if(dbReview == null)
                    return ServiceResult<bool>.Failure("Review Does not exist.");

                if(dbReview.UserId != userId)
                    return ServiceResult<bool>.Failure("You are not authorized to delete this review.");

                _unitOfWork.ReviewRepository.Delete(dbReview);
                await _unitOfWork.SaveChangesAsync();

                var success = await UpdateCourseAvg(dbReview.CourseId);
                if(success)
                {
                    return ServiceResult<bool>.Success(true, "Review Deleted Successfully");
                }

                return ServiceResult<bool>.Failure("Something wrong happened");
            }
            catch
            {
                return ServiceResult<bool>.Failure("Something wrong happened");
            }
        }

        public async Task<ServiceResult<PaginatedList<ReviewDTO>>> GetReviewsByCourseIdAsync(int courseId, int? userId, int index)
        {
            try
            {
                var list = await _unitOfWork.ReviewRepository.GetReviewsByCourseIdAsync(courseId, userId, index);
                list.ForEach(x =>
                {
                    x.ImageMetadata.ImageURL = _cloudinaryService
                        .GetImageUrl(x.ImageMetadata.ImageURL, x.ImageMetadata.Version);
                });
                return ServiceResult<PaginatedList<ReviewDTO>>.Success(list, "Reviews are retrieved successfully.");
            }
            catch
            {
                return ServiceResult<PaginatedList<ReviewDTO>>.Failure("Something went wrong.");
            }
        }

        public async Task<ServiceResult<ReviewDTO>> EditReviewAsync(int userId, EditReviewRequestModel review)
        {
            try
            {
                var courseId = _unitOfWork.ReviewRepository.EditReview(userId, review);
                await _unitOfWork.SaveChangesAsync();

                var success = await UpdateCourseAvg(courseId);
                var newReviewRes = await _unitOfWork.ReviewRepository.GetStudentCourseReview(courseId, userId);

                if (success && newReviewRes.TryGetData(out var newReview))
                {
                    return ServiceResult<ReviewDTO>.Success(newReview, "Review was edited successfully");
                }
                return ServiceResult<ReviewDTO>.Failure("Something wrong happened");

            }
            catch(ArgumentException ex)
            {
                return ServiceResult<ReviewDTO>.Failure(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return ServiceResult<ReviewDTO>.Failure(ex.Message);
            }
            catch
            {
                return ServiceResult<ReviewDTO>.Failure("Something wrong happened");
            }
        }
    }
}

