using GraduationProject.API.Requests;
using GraduationProject.Domain.DTOs;

namespace GraduationProject.Application.Services.Interfaces
{
    public interface IReviewService
    {
        Task<ServiceResult<ReviewDTO>> AddReviewAsync(int userId, AddReviewRequestModel review);
        Task<ServiceResult<ReviewDTO>> EditReviewAsync(int userId, EditReviewRequestModel review);
        Task<ServiceResult<bool>> DeleteReviewAsync(int userId, int reviewId);
        Task<ServiceResult<PaginatedList<ReviewDTO>>> GetReviewsByCourseIdAsync(int courseId, int? userId, int index);
    }
}

