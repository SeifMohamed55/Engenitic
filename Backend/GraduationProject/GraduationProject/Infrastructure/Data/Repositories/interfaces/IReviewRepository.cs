using GraduationProject.API.Requests;
using GraduationProject.Application.Services;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data.Repositories.Base.Interfaces;

namespace GraduationProject.Infrastructure.Data.Repositories.interfaces
{
    public interface IReviewRepository : IBulkRepository<Review, int>, ICustomRepository
    {
        Task<RatingStatsDTO> GetCourseRatingStats(int courseId);
        Task<PaginatedList<ReviewDTO>> GetReviewsByCourseIdAsync(int courseId, int? userId, int page);
        void AddReview(int userId, AddReviewRequestModel review);
        Task<double> GetAverageCourseRatingAsync(int courseId);
        int EditReview(int userId, EditReviewRequestModel review);
        Task<bool> ReviewExist(int userId, int courseId);
    }

}
