using System.ComponentModel.DataAnnotations;

namespace GraduationProject.API.Requests
{
    public class EditReviewRequestModel
    {
        [Required]
        public int ReviewId { get; set; }

        [Required]  
        public int CourseId { get; set; }
        [Required]
        [StringLength(4096, ErrorMessage = "Content cannot be longer than 4096 characters.")]
        public string Content { get; set; } = null!;
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public byte Rating { get; set; }
    }
}
