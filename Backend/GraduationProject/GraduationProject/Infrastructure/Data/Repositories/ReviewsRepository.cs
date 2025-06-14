using GraduationProject.API.Requests;
using GraduationProject.Application.Services;
using GraduationProject.Common.Extensions;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data.Repositories.Base;
using GraduationProject.Infrastructure.Data.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Infrastructure.Data.Repositories
{

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
            
            var finalList = await PaginatedList<ReviewDTO>.CreateAsync(query, index);
            

            return finalList;
        }

        public async Task<RatingStatsDTO> GetCourseRatingStats(int courseId)
        {
            var grouped = await _dbSet
                .Where(r => r.CourseId == courseId)
                .GroupBy(r => 1) // everything in one group
                .Select(g => new
                {
                    Avg = g.Average(r => (float?)r.Rating) ?? 0.0f,
                    Ratings = g.GroupBy(r => r.Rating)
                               .Select(rg => new { Rating = rg.Key, Count = rg.Count() })
                               .ToList()
                })
                .FirstOrDefaultAsync();

            if(grouped == null)
            {
                var emptyDic = new Dictionary<byte, CourseStatDTO>();
                for (byte i = 1; i <= 5; i++)
                {   
                    emptyDic[i] = new CourseStatDTO(0, 0.0f);
                }
                return new RatingStatsDTO(0.0f, emptyDic);
            }

            var dict = grouped.Ratings
                .ToDictionary(
                    x => x.Rating,
                    x => new CourseStatDTO(x.Count, float.Round(((float)x.Count / grouped.Ratings.Sum(x => x.Count)) * 100.0f, 1))
                );

            for (byte i = 1; i <= 5; i++)
            {
                if (!dict.ContainsKey(i))
                {
                    dict[i] = new CourseStatDTO(0, 0.0f);
                }
            }
            return new RatingStatsDTO(float.Round(grouped.Avg, 1), dict);
        }

        public void AddReview(int userId, AddReviewRequestModel review)
        {
            var reviewDb = new Review(userId, review);
            Insert(reviewDb);
        }

        public async Task<ServiceResult<ReviewDTO>> GetStudentCourseReview(int courseId, int studentId)
        {
            var dto = await _dbSet.DTOProjection()
                .FirstOrDefaultAsync(x => x.CourseId == courseId && x.UserId == studentId);

            return dto != null
                ? ServiceResult<ReviewDTO>.Success(dto, "Review retrieved successfully")
                : ServiceResult<ReviewDTO>.Failure("Review not found");
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
            reviewDb.UpdateFromRequest(review.Content, review.Rating);
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
