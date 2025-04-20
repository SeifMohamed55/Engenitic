using System.ComponentModel.DataAnnotations;

namespace GraduationProject.API.Requests
{
    public class DeleteReviewRequest
    {
        [Required]
        public int ReviewId { get; set; }
    }
}
