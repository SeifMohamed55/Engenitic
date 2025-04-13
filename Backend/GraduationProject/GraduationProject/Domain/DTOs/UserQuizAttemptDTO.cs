using System.ComponentModel.DataAnnotations;

namespace GraduationProject.Domain.DTOs
{
    public class UserQuizAttemptDTO
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public int EnrollmentId { get; set; }
        [Required]
        public int QuizId { get; set; }
        [Required]
        [NotEmptyCollection]
        [MinLength(1, ErrorMessage = "At least one answer is required.")]
        public List<UserAnswerDTO> UserAnswers { get; set; } = new List<UserAnswerDTO>();

        public bool IsPassed { get; set; } = false;
    }
}
