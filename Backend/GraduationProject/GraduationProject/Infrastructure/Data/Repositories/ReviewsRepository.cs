using GraduationProject.API.Requests;
using GraduationProject.Application.Services;
using GraduationProject.Common.Extensions;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using System.Collections.Frozen;
using System.Linq;

namespace GraduationProject.Infrastructure.Data.Repositories
{

    public interface IReviewRepository : IBulkRepository<Review, int>
    {
        Task<RatingStats> GetCourseRatingStats(int courseId);
        Task<PaginatedList<ReviewDTO>> GetReviewsByCourseIdAsync(int courseId, int? userId, int page);
        void AddReview(int userId, AddReviewRequestModel review);
        Task<double> GetAverageCourseRatingAsync(int courseId);
        int EditReview(int userId, EditReviewRequestModel review);
        Task<bool> ReviewExist(int userId, int courseId);
    }

    public class ReviewsRepository : BulkRepository<Review, int>, IReviewRepository
    {
        public ReviewsRepository(AppDbContext context) : base(context)
        {

        }

        public async Task<PaginatedList<ReviewDTO>> GetReviewsByCourseIdAsync(int courseId, int? userId, int index)
        {
            var userReview = userId != null
                ? await _dbSet.DTOProjection().FirstOrDefaultAsync(x => x.UserId == userId && x.CourseId == courseId)
                : null;

            var query = _dbSet
                .DTOProjection();

            PaginatedList<ReviewDTO> finalList;

            if (userReview != null && index == 1)
            {
                finalList = await PaginatedList<ReviewDTO>.CreateAsync(query, index, 9);
                finalList.Prepend(userReview);
            }
            else
            {
                finalList = await PaginatedList<ReviewDTO>.CreateAsync(query, index);
            }

            return finalList;
        }

        public async Task<RatingStats> GetCourseRatingStats(int courseId)
        {
            var grouped = await _dbSet
                .Where(r => r.CourseId == courseId)
                .GroupBy(r => 1) // everything in one group
                .Select(g => new
                {
                    Avg = g.Average(r => (double?)r.Rating) ?? 0.0,
                    Ratings = g.GroupBy(r => r.Rating)
                               .Select(rg => new { Rating = rg.Key, Count = rg.Count() })
                               .ToList()
                })
                .FirstOrDefaultAsync();

            var dict = grouped?.Ratings.ToDictionary(x => x.Rating, x => x.Count) ?? new Dictionary<byte, int>();
            for (byte i = 1; i <= 5; i++)
            {
                if (!dict.ContainsKey(i))
                {
                    dict[i] = 0;
                }
            }
            return new RatingStats(grouped?.Avg ?? 0.0, dict);
        }

        public void AddReview(int userId, AddReviewRequestModel review)
        {
            var reviewDb = new Review(userId, review);
            Insert(reviewDb);
        }

        public int EditReview(int userId, EditReviewRequestModel review)
        {
            var reviewDb = _dbSet.Find(review.ReviewId);
            if(reviewDb == null)
            {
                throw new ArgumentException("Review not found");
            }
            if(reviewDb.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to edit this review");
            }
            reviewDb.UpdateFromRequest(reviewDb.Content, review.Rating);
            return reviewDb.CourseId;
        }

        public async Task<double> GetAverageCourseRatingAsync(int courseId)
        {
            return await _dbSet
                .Where(r => r.CourseId == courseId)
                .AverageAsync(r => (double?)r.Rating) ?? 0.0;
        }

        public async Task<bool> ReviewExist(int userId, int courseId)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.UserId == userId && x.CourseId == courseId) != null;

        }
    }

}
