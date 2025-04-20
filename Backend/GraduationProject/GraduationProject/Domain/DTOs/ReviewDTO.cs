using GraduationProject.Domain.Models;

namespace GraduationProject.Domain.DTOs
{
    public class ReviewDTO
    {
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public string Content { get; set; } = null!;
        public DateTime UpdatedAt { get; set; }
        public byte Rating { get; set; }
        public string FullName { get; set; } = null!;
        public ImageMetadata ImageMetadata { get; set; } = null!;

    }
}
