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
        Task<PaginatedList<ReviewDTO>> GetReviewsByCourseIdAsync(int courseId, int page);
        void AddReview(AddReviewRequestModel review);
        Task<double> GetAverageCourseRatingAsync(int courseId);
        void EditReview(EditReviewRequestModel review);
    }

    public class ReviewsRepository : BulkRepository<Review, int>, IReviewRepository
    {
        public ReviewsRepository(AppDbContext context) : base(context)
        {
        }

        public Task<PaginatedList<ReviewDTO>> GetReviewsByCourseIdAsync(int courseId, int index)
        {
            var query = _dbSet
                .Include(x => x.User) // only top-level
                .DTOProjection();

            return PaginatedList<ReviewDTO>.CreateAsync(query, index);
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

            var dict = grouped?.Ratings.ToFrozenDictionary(x => x.Rating, x => x.Count) ?? FrozenDictionary<byte, int>.Empty;
            return new RatingStats(grouped?.Avg ?? 0.0, dict);
        }

        public void AddReview(AddReviewRequestModel review)
        {
            var reviewDb = new Review(review);
            Insert(reviewDb);
        }

        public void EditReview(EditReviewRequestModel review)
        {
            var reviewDb = _dbSet.Find(review.ReviewId);
            if(reviewDb == null)
            {
                throw new ArgumentNullException("Review not found");
            }
            reviewDb.UpdateFromRequest(reviewDb.Content, review.Rating);
        }

        public async Task<double> GetAverageCourseRatingAsync(int courseId)
        {
            return await _dbSet
                .Where(r => r.CourseId == courseId)
                .AverageAsync(r => (double?)r.Rating) ?? 0.0;
        }

    }

}
