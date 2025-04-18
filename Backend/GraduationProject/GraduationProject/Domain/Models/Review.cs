using GraduationProject.API.Requests;
using GraduationProject.Domain.DTOs;

namespace GraduationProject.Domain.Models
{
    public class Review : IEntity<int>
    {
        public Review() { }

        public Review(AddReviewRequestModel req)
        {
            Content = req.Content;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            Rating = req.Rating;
            UserId = req.UserId;
            CourseId = req.CourseId;
        }
        public int Id { get; set; }
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public byte Rating { get; set; }

        public int UserId { get; set; }
        public AppUser User { get; set; } = null!;

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public void UpdateFromRequest(string newContent, byte rating)
        {
            Content = newContent;
            Rating = rating;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
